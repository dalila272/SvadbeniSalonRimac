import 'package:flutter/material.dart';
import 'package:svadbeni_salon_rimac/models/review.dart';
import 'package:svadbeni_salon_rimac/models/wedding.dart';
import 'package:svadbeni_salon_rimac/providers/review_provider.dart';
import 'package:svadbeni_salon_rimac/providers/wedding_provider.dart';
import 'package:svadbeni_salon_rimac/utils/master_screen.dart';

class ReviewScreen extends StatefulWidget {
  const ReviewScreen({super.key});

  @override
  State<ReviewScreen> createState() => _ReviewScreenState();
}

class _ReviewScreenState extends State<ReviewScreen> {
  final _reviewProvider = ReviewProvider();
  final _weddingProvider = WeddingProvider();

  Review? _existingReview;
  Wedding? _eligibleWedding;
  int _selectedRating = 0;
  final _commentController = TextEditingController();
  bool _isLoading = true;
  bool _isSaving = false;
  String _errorMessage = '';

  @override
  void dispose() {
    _commentController.dispose();
    super.dispose();
  }

  @override
  void initState() {
    super.initState();
    _loadData();
  }

  Future<void> _loadData() async {
    setState(() {
      _isLoading = true;
      _errorMessage = '';
    });

    try {
      final reviewsResult = await _reviewProvider.get();
      final weddingsResult = await _weddingProvider.get();

      Review? latestReview;
      if (reviewsResult.items.isNotEmpty) {
        latestReview = reviewsResult.items.first;
      }

      Wedding? eligible;
      for (final wedding in weddingsResult.items) {
        // Samo završena svadba (Completed = 3)
        if (wedding.status == 3) {
          final alreadyReviewed = reviewsResult.items
              .any((review) => review.weddingId == wedding.id);
          if (!alreadyReviewed) {
            eligible = wedding;
            break;
          }
        }
      }

      setState(() {
        _existingReview = latestReview;
        _eligibleWedding = eligible;
        _isLoading = false;
      });
    } catch (e) {
      setState(() {
        _isLoading = false;
        _errorMessage = e.toString().replaceFirst('Exception: ', '');
      });
    }
  }

  Future<void> _submitReview() async {
    if (_eligibleWedding == null || _selectedRating == 0) return;

    setState(() => _isSaving = true);
    try {
      await _reviewProvider.insert({
        'ponudaId': _eligibleWedding!.offerId,
        'svadbaId': _eligibleWedding!.id,
        'ocjena': _selectedRating,
        'komentar': _commentController.text.trim(),
      });
      await _loadData();
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Hvala vam na ocjeni!')),
        );
      }
    } catch (e) {
      setState(() {
        _errorMessage = e.toString().replaceFirst('Exception: ', '');
      });
    } finally {
      if (mounted) setState(() => _isSaving = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    return MasterScreenWidget(
      title: 'Ocjena paketa',
      child: _isLoading
          ? const Center(child: CircularProgressIndicator())
          : SingleChildScrollView(
              padding: const EdgeInsets.all(16),
              child: Center(child: _buildContent()),
            ),
    );
  }

  Widget _buildContent() {
    if (_errorMessage.isNotEmpty) {
      return Text(_errorMessage, style: const TextStyle(color: Colors.red));
    }

    if (_existingReview != null && _eligibleWedding == null) {
      return _buildRatedView(_existingReview!);
    }

    if (_eligibleWedding != null) {
      return _buildRatingForm(_eligibleWedding!);
    }

    return const Text(
      'Recenziju možete ostaviti nakon što admin označi vašu svadbu kao završenu.',
      textAlign: TextAlign.center,
      style: TextStyle(fontSize: 16),
    );
  }

  Widget _buildRatedView(Review review) {
    return Column(
      mainAxisAlignment: MainAxisAlignment.center,
      children: [
        Text(
          'Ocijenili ste "${review.offerName}" paket sa ocjenom ${review.rating}.',
          style: const TextStyle(fontSize: 18),
          textAlign: TextAlign.center,
        ),
        const SizedBox(height: 16),
        _starRow(review.rating, interactive: false),
        if (review.comment != null && review.comment!.isNotEmpty) ...[
          const SizedBox(height: 16),
          Text(
            review.comment!,
            textAlign: TextAlign.center,
            softWrap: true,
          ),
        ],
        const SizedBox(height: 40),
        const Text(
          'Hvala vam što ste bili naš gost!',
          style: TextStyle(fontSize: 16, fontStyle: FontStyle.italic),
        ),
      ],
    );
  }

  Widget _buildRatingForm(Wedding wedding) {
    return Column(
      mainAxisAlignment: MainAxisAlignment.center,
      children: [
        const Text(
          'Naziv korištenog paketa:',
          style: TextStyle(fontWeight: FontWeight.bold),
        ),
        const SizedBox(height: 8),
        Text(wedding.offerName, style: const TextStyle(fontSize: 18)),
        const SizedBox(height: 24),
        const Text('Molimo Vas ocijenite paket'),
        const SizedBox(height: 8),
        _starRow(_selectedRating, interactive: true),
        const SizedBox(height: 16),
        TextField(
          controller: _commentController,
          maxLines: 3,
          decoration: const InputDecoration(
            hintText: 'Komentar (opcionalno)',
            border: OutlineInputBorder(),
          ),
        ),
        const SizedBox(height: 24),
        ElevatedButton(
          onPressed: _isSaving || _selectedRating == 0 ? null : _submitReview,
          child: _isSaving
              ? const SizedBox(
                  width: 24,
                  height: 24,
                  child: CircularProgressIndicator(strokeWidth: 2),
                )
              : const Text('Ocijeni'),
        ),
      ],
    );
  }

  Widget _starRow(int rating, {required bool interactive}) {
    return Row(
      mainAxisAlignment: MainAxisAlignment.center,
      children: List.generate(5, (index) {
        if (interactive) {
          return IconButton(
            icon: Icon(
              Icons.star,
              color: index < rating ? Colors.amber : Colors.grey,
            ),
            onPressed: () => setState(() => _selectedRating = index + 1),
          );
        }
        return Icon(
          Icons.star,
          color: index < rating ? Colors.amber : Colors.grey,
          size: 30,
        );
      }),
    );
  }
}
