import 'package:flutter/material.dart';
import 'package:svadbeni_salon_rimac/constants/colors.dart';
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

  List<Review> _myReviews = [];
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
      // Učitaj odvojeno da jedna greška ne sakrije ocjene.
      List<Review> reviews = [];
      List<Wedding> completed = [];
      String? reviewsError;
      String? weddingsError;

      try {
        final reviewsResult = await _reviewProvider.get(filter: {
          'Page': 1,
          'PageSize': 100,
          'IncludeTotalCount': true,
        });
        reviews = reviewsResult.items;
      } catch (e) {
        reviewsError = e.toString().replaceFirst('Exception: ', '');
      }

      try {
        final completedWeddings = await _weddingProvider.get(filter: {
          'status': 3,
          'Page': 1,
          'PageSize': 100,
          'IncludeTotalCount': true,
        });
        completed = completedWeddings.items;
      } catch (e) {
        weddingsError = e.toString().replaceFirst('Exception: ', '');
      }

      // Ako je naziv paketa prazan iz API-ja, popuni ga iz svadbe.
      reviews = reviews.map((r) {
        if (r.offerName.trim().isNotEmpty) return r;
        Wedding? match;
        for (final w in completed) {
          if (w.id == r.weddingId || w.offerId == r.offerId) {
            match = w;
            break;
          }
        }
        if (match == null) return r;
        return Review(
          id: r.id,
          offerId: r.offerId,
          offerName: match.offerName,
          weddingId: r.weddingId,
          rating: r.rating,
          comment: r.comment,
          createdAt: r.createdAt,
        );
      }).toList();

      Wedding? eligible;
      for (final wedding in completed) {
        final alreadyReviewed = reviews.any(
          (review) =>
              review.weddingId == wedding.id ||
              review.offerId == wedding.offerId,
        );
        if (!alreadyReviewed) {
          eligible = wedding;
          break;
        }
      }

      if (!mounted) return;
      setState(() {
        _myReviews = reviews;
        _eligibleWedding = eligible;
        _selectedRating = 0;
        _commentController.clear();
        _isLoading = false;
        if (reviews.isEmpty &&
            eligible == null &&
            (reviewsError != null || weddingsError != null)) {
          _errorMessage = [
            if (reviewsError != null) reviewsError,
            if (weddingsError != null) weddingsError,
          ].join('\n');
        }
      });
    } catch (e) {
      if (!mounted) return;
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
          : RefreshIndicator(
              onRefresh: _loadData,
              color: AppColors.primaryColor,
              child: ListView(
                physics: const AlwaysScrollableScrollPhysics(),
                padding: const EdgeInsets.all(20),
                children: [_buildContent()],
              ),
            ),
    );
  }

  Widget _buildContent() {
    if (_errorMessage.isNotEmpty && _myReviews.isEmpty && _eligibleWedding == null) {
      return Column(
        children: [
          Text(_errorMessage, style: const TextStyle(color: Colors.red)),
          const SizedBox(height: 12),
          TextButton(onPressed: _loadData, child: const Text('Pokušaj ponovo')),
        ],
      );
    }

    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        if (_eligibleWedding != null) ...[
          _buildRatingForm(_eligibleWedding!),
          if (_myReviews.isNotEmpty) const SizedBox(height: 28),
        ] else if (_myReviews.isEmpty)
          _buildWaitingState()
        else
          _buildAlreadyRatedHeader(),
        ..._myReviews.map(_buildReviewCard),
      ],
    );
  }

  Widget _buildWaitingState() {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 28),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(16),
        boxShadow: [
          BoxShadow(
            color: AppColors.primaryColor.withValues(alpha: 0.08),
            blurRadius: 12,
            offset: const Offset(0, 4),
          ),
        ],
      ),
      child: Column(
        children: [
          Icon(
            Icons.event_available_outlined,
            size: 48,
            color: AppColors.primaryColor.withValues(alpha: 0.7),
          ),
          const SizedBox(height: 16),
          const Text(
            'Opcija za ocjenjivanje paketa bit će moguća kada se svadba završi.',
            textAlign: TextAlign.center,
            style: TextStyle(fontSize: 16, height: 1.4),
          ),
          const SizedBox(height: 20),
          _starRow(0, interactive: false, size: 34),
          const SizedBox(height: 12),
          Text(
            'Ako očekujete postojeću ocjenu, prijavite se kao mobile / test.',
            textAlign: TextAlign.center,
            style: TextStyle(fontSize: 12, color: Colors.grey[600]),
          ),
        ],
      ),
    );
  }

  Widget _buildAlreadyRatedHeader() {
    return Padding(
      padding: const EdgeInsets.only(bottom: 16),
      child: Column(
        children: [
          Icon(
            Icons.check_circle_outline,
            size: 44,
            color: AppColors.primaryColor.withValues(alpha: 0.85),
          ),
          const SizedBox(height: 12),
          const Text(
            'Vaša ocjena paketa',
            textAlign: TextAlign.center,
            style: TextStyle(fontSize: 16, fontWeight: FontWeight.w600),
          ),
          const SizedBox(height: 6),
          Text(
            'Nova ocjena bit će moguća kada se sljedeća svadba završi.',
            textAlign: TextAlign.center,
            style: TextStyle(fontSize: 13, color: Colors.grey[600]),
          ),
        ],
      ),
    );
  }

  Widget _buildReviewCard(Review review) {
    final title = review.offerName.trim().isEmpty
        ? 'Paket #${review.offerId}'
        : review.offerName;

    return Card(
      elevation: 2,
      margin: const EdgeInsets.only(bottom: 12),
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
      child: Padding(
        padding: const EdgeInsets.all(18),
        child: Column(
          children: [
            Text(
              title,
              style: const TextStyle(
                fontSize: 18,
                fontWeight: FontWeight.w700,
                color: AppColors.primaryColor,
              ),
              textAlign: TextAlign.center,
            ),
            const SizedBox(height: 12),
            _starRow(review.rating, interactive: false, size: 34),
            const SizedBox(height: 8),
            Text(
              '${review.rating} / 5',
              style: TextStyle(
                color: Colors.grey[700],
                fontWeight: FontWeight.w600,
              ),
            ),
            if (review.comment != null && review.comment!.trim().isNotEmpty) ...[
              const SizedBox(height: 14),
              Text(
                review.comment!,
                textAlign: TextAlign.center,
                style: TextStyle(
                  fontSize: 14,
                  height: 1.4,
                  color: Colors.grey[800],
                ),
              ),
            ],
          ],
        ),
      ),
    );
  }

  Widget _buildRatingForm(Wedding wedding) {
    return Container(
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(16),
        boxShadow: [
          BoxShadow(
            color: AppColors.primaryColor.withValues(alpha: 0.08),
            blurRadius: 12,
            offset: const Offset(0, 4),
          ),
        ],
      ),
      child: Column(
        children: [
          const Text(
            'Naziv korištenog paketa',
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
      ),
    );
  }

  Widget _starRow(int rating, {required bool interactive, double size = 30}) {
    return Row(
      mainAxisAlignment: MainAxisAlignment.center,
      children: List.generate(5, (index) {
        final filled = index < rating;
        final icon = Icon(
          filled ? Icons.star : Icons.star_border,
          color: filled ? Colors.amber.shade700 : Colors.grey.shade400,
          size: size,
        );
        if (interactive) {
          return IconButton(
            icon: icon,
            onPressed: () => setState(() => _selectedRating = index + 1),
          );
        }
        return Padding(
          padding: const EdgeInsets.symmetric(horizontal: 2),
          child: icon,
        );
      }),
    );
  }
}
