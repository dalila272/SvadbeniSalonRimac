import 'package:flutter/material.dart';
import 'package:svadbeni_salon_rimac/models/offer_detail.dart';
import 'package:svadbeni_salon_rimac/providers/offer_detail_provider.dart';

class PackageDetailsScreen extends StatefulWidget {
  final int offerId;
  final String? title;

  const PackageDetailsScreen({
    super.key,
    required this.offerId,
    this.title,
  });

  @override
  State<PackageDetailsScreen> createState() => _PackageDetailsScreenState();
}

class _PackageDetailsScreenState extends State<PackageDetailsScreen> {
  final _provider = OfferDetailProvider();
  late Future<OfferDetail> _offerFuture;

  @override
  void initState() {
    super.initState();
    _offerFuture = _provider.getById(widget.offerId);
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: Text(widget.title ?? 'Detalji ponude'),
      ),
      body: FutureBuilder<OfferDetail>(
        future: _offerFuture,
        builder: (context, snapshot) {
          if (snapshot.connectionState == ConnectionState.waiting) {
            return const Center(child: CircularProgressIndicator());
          }
          if (snapshot.hasError) {
            return Center(
              child: Text(
                'Greška: ${snapshot.error}',
                style: const TextStyle(color: Colors.red),
                textAlign: TextAlign.center,
              ),
            );
          }

          final offer = snapshot.data!;
          return SingleChildScrollView(
            padding: const EdgeInsets.all(16),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  offer.title,
                  style: const TextStyle(
                    fontSize: 24,
                    fontWeight: FontWeight.bold,
                  ),
                ),
                const SizedBox(height: 8),
                Text(
                  'Cijena: ${offer.price} KM',
                  style: const TextStyle(fontSize: 18, color: Colors.green),
                ),
                const SizedBox(height: 12),
                Text(
                  offer.description,
                  style: const TextStyle(fontSize: 16, color: Colors.black54),
                ),
                if (offer.menu != null) ...[
                  const SizedBox(height: 24),
                  _sectionTitle('Meni'),
                  Text(
                    offer.menu!.name,
                    style: const TextStyle(
                      fontSize: 18,
                      fontWeight: FontWeight.w600,
                    ),
                  ),
                  if (offer.menu!.description.isNotEmpty)
                    Text(offer.menu!.description),
                  Text('Cijena menija: ${offer.menu!.price} KM'),
                ],
                if (offer.musicians.isNotEmpty) ...[
                  const SizedBox(height: 24),
                  _sectionTitle('Muzičari'),
                  ...offer.musicians.map(
                    (m) => ListTile(
                      contentPadding: EdgeInsets.zero,
                      title: Text(m.name),
                      subtitle:
                          m.description.isNotEmpty ? Text(m.description) : null,
                    ),
                  ),
                ],
                if (offer.decorations.isNotEmpty) ...[
                  const SizedBox(height: 24),
                  _sectionTitle('Dekoracije'),
                  ...offer.decorations.map(
                    (d) => ListTile(
                      contentPadding: EdgeInsets.zero,
                      title: Text(d.name),
                      subtitle: Text(
                        d.description.isNotEmpty
                            ? '${d.description} (${d.price} KM)'
                            : '${d.price} KM',
                      ),
                    ),
                  ),
                ],
              ],
            ),
          );
        },
      ),
    );
  }

  Widget _sectionTitle(String text) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 8),
      child: Text(
        text,
        style: const TextStyle(
          fontSize: 20,
          fontWeight: FontWeight.bold,
          color: Color.fromRGBO(135, 82, 82, 0.8),
        ),
      ),
    );
  }
}
