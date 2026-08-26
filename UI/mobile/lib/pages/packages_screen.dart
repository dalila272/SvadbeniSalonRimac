import 'package:flutter/material.dart';
import 'package:svadbeni_salon_rimac/pages/package_details_screen.dart';
import 'package:svadbeni_salon_rimac/providers/packages_provider.dart';
import 'package:svadbeni_salon_rimac/models/package.dart';
import 'package:svadbeni_salon_rimac/utils/master_screen.dart';

class PackagesScreen extends StatefulWidget {
  const PackagesScreen({super.key});

  @override
  State<PackagesScreen> createState() => PackagesScreenState();
}

class PackagesScreenState extends State<PackagesScreen> {
  late Future<List<Package>> _packagesFuture;

  @override
  void initState() {
    super.initState();
    _packagesFuture = _loadPackages();
  }

  Future<List<Package>> _loadPackages() async {
    final provider = PackagesProvider();
    final result = await provider.get(filter: {
      'IncludeTotalCount': true,
      'Page': 1,
      'PageSize': 100,
    });
    return result.items;
  }

  @override
  Widget build(BuildContext context) {
    return MasterScreenWidget(
      title_widget: "Ponude",
      child: Padding(
        padding: const EdgeInsets.all(8.0),
        child: FutureBuilder<List<Package>>(
          future: _packagesFuture,
          builder: (context, snapshot) {
            if (snapshot.connectionState == ConnectionState.waiting) {
              return const Center(child: CircularProgressIndicator());
            }
            if (snapshot.hasError) {
              return Center(
                child: Text(
                  'Došlo je do greške: ${snapshot.error}',
                  style: const TextStyle(color: Colors.red),
                  textAlign: TextAlign.center,
                ),
              );
            }
            final packages = snapshot.data ?? [];
            if (packages.isEmpty) {
              return const Center(child: Text('Nema dostupnih ponuda.'));
            }
            return ListView.builder(
              itemCount: packages.length,
              itemBuilder: (context, index) => _packageTile(packages[index]),
            );
          },
        ),
      ),
    );
  }

  Widget _packageTile(Package ponuda) {
    return Card(
      elevation: 5,
      margin: const EdgeInsets.symmetric(vertical: 8.0),
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
      child: TextButton(
        onPressed: () {
          Navigator.push(
            context,
            MaterialPageRoute(
              builder: (context) => PackageDetailsScreen(
                offerId: ponuda.id,
                title: ponuda.title,
              ),
            ),
          );
        },
        child: Padding(
          padding: const EdgeInsets.all(16.0),
          child: Text(
            '${ponuda.title} (${ponuda.price} KM)',
            style: const TextStyle(
              fontSize: 18.0,
              fontWeight: FontWeight.w600,
              color: Color.fromRGBO(135, 82, 82, 0.6),
            ),
          ),
        ),
      ),
    );
  }
}
