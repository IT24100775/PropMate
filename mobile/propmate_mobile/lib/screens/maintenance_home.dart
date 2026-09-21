import 'package:flutter/material.dart';
import 'create_request.dart';

class MaintenanceHome extends StatefulWidget {
  const MaintenanceHome({super.key});

  @override
  State<MaintenanceHome> createState() => _MaintenanceHomeState();
}

class _MaintenanceHomeState extends State<MaintenanceHome> {
  // Temporary data for the frontend.
  // We will connect this to the ASP.NET API next.
  final List<Map<String, String>> requests = [
    {
      'id': '5',
      'description': 'Water is leaking from the bathroom pipe',
      'category': 'Plumbing',
      'priority': 'HIGH',
      'status': 'PENDING',
    },
    {
      'id': '6',
      'description': 'Bedroom light is not working',
      'category': 'Electrical',
      'priority': 'MEDIUM',
      'status': 'IN_PROGRESS',
    },
  ];

  Color getPriorityColor(String priority) {
    switch (priority) {
      case 'HIGH':
        return Colors.red;
      case 'MEDIUM':
        return Colors.orange;
      case 'LOW':
        return Colors.green;
      default:
        return Colors.grey;
    }
  }

  Color getStatusColor(String status) {
    switch (status) {
      case 'PENDING':
        return Colors.orange;
      case 'ASSIGNED':
        return Colors.blue;
      case 'SCHEDULED':
        return Colors.indigo;
      case 'IN_PROGRESS':
        return Colors.deepPurple;
      case 'RESOLVED':
        return Colors.green;
      default:
        return Colors.grey;
    }
  }

  @override
  Widget build(BuildContext context) {
    final pendingCount =
        requests.where((r) => r['status'] == 'PENDING').length;

    final inProgressCount =
        requests.where((r) => r['status'] == 'IN_PROGRESS').length;

    return Scaffold(
      backgroundColor: const Color(0xfff5f7fb),

      appBar: AppBar(
        title: const Text(
          'PropMate',
          style: TextStyle(
            fontWeight: FontWeight.bold,
          ),
        ),
        backgroundColor: Colors.white,
        foregroundColor: Colors.black87,
        elevation: 0,
      ),

      body: RefreshIndicator(
        onRefresh: () async {
          // API refresh will be added later.
          await Future.delayed(
            const Duration(milliseconds: 500),
          );

          setState(() {});
        },

        child: ListView(
          padding: const EdgeInsets.all(20),
          children: [

            // Header
            const Text(
              'Maintenance',
              style: TextStyle(
                fontSize: 28,
                fontWeight: FontWeight.bold,
              ),
            ),

            const SizedBox(height: 6),

            Text(
              'Manage your maintenance requests',
              style: TextStyle(
                fontSize: 15,
                color: Colors.grey.shade600,
              ),
            ),

            const SizedBox(height: 24),

            // Statistics
            Row(
              children: [

                Expanded(
                  child: _buildStatCard(
                    title: 'Total',
                    value: requests.length.toString(),
                    icon: Icons.build_circle_outlined,
                    color: Colors.indigo,
                  ),
                ),

                const SizedBox(width: 12),

                Expanded(
                  child: _buildStatCard(
                    title: 'Pending',
                    value: pendingCount.toString(),
                    icon: Icons.pending_actions,
                    color: Colors.orange,
                  ),
                ),

                const SizedBox(width: 12),

                Expanded(
                  child: _buildStatCard(
                    title: 'Progress',
                    value: inProgressCount.toString(),
                    icon: Icons.engineering_outlined,
                    color: Colors.deepPurple,
                  ),
                ),
              ],
            ),

            const SizedBox(height: 28),

            // New Request button
            SizedBox(
              width: double.infinity,
              child: ElevatedButton.icon(
                onPressed: () async {
                  await Navigator.push(
                    context,
                    MaterialPageRoute(
                      builder: (context) =>
                          const CreateRequestScreen(),
                    ),
                  );
                },
                icon: const Icon(Icons.add),
                label: const Text(
                  'Create Maintenance Request',
                  style: TextStyle(
                    fontSize: 16,
                    fontWeight: FontWeight.w600,
                  ),
                ),
                style: ElevatedButton.styleFrom(
                  padding: const EdgeInsets.symmetric(
                    vertical: 16,
                  ),
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(12),
                  ),
                ),
              ),
            ),

            const SizedBox(height: 28),

            // Section title
            const Text(
              'My Maintenance Requests',
              style: TextStyle(
                fontSize: 20,
                fontWeight: FontWeight.bold,
              ),
            ),

            const SizedBox(height: 12),

            // Request cards
            ...requests.map(
              (request) => _buildRequestCard(request),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildStatCard({
    required String title,
    required String value,
    required IconData icon,
    required Color color,
  }) {
    return Container(
      padding: const EdgeInsets.all(14),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(14),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(0.05),
            blurRadius: 10,
            offset: const Offset(0, 4),
          ),
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [

          Icon(
            icon,
            color: color,
            size: 26,
          ),

          const SizedBox(height: 10),

          Text(
            value,
            style: const TextStyle(
              fontSize: 24,
              fontWeight: FontWeight.bold,
            ),
          ),

          Text(
            title,
            style: TextStyle(
              color: Colors.grey.shade600,
              fontSize: 13,
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildRequestCard(
    Map<String, String> request,
  ) {
    final priority = request['priority'] ?? '';
    final status = request['status'] ?? '';

    return Container(
      margin: const EdgeInsets.only(bottom: 14),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(16),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(0.04),
            blurRadius: 10,
            offset: const Offset(0, 3),
          ),
        ],
      ),
      child: InkWell(
        borderRadius: BorderRadius.circular(16),
        onTap: () {
          // Request details screen will be connected next.
          ScaffoldMessenger.of(context).showSnackBar(
            SnackBar(
              content: Text(
                'Opening request #${request['id']}',
              ),
            ),
          );
        },
        child: Padding(
          padding: const EdgeInsets.all(16),
          child: Column(
            crossAxisAlignment:
                CrossAxisAlignment.start,
            children: [

              Row(
                mainAxisAlignment:
                    MainAxisAlignment.spaceBetween,
                children: [

                  Text(
                    '#${request['id']}',
                    style: TextStyle(
                      fontWeight: FontWeight.bold,
                      color: Colors.grey.shade600,
                    ),
                  ),

                  Container(
                    padding: const EdgeInsets.symmetric(
                      horizontal: 10,
                      vertical: 5,
                    ),
                    decoration: BoxDecoration(
                      color: getStatusColor(status)
                          .withOpacity(0.1),
                      borderRadius:
                          BorderRadius.circular(20),
                    ),
                    child: Text(
                      status,
                      style: TextStyle(
                        color: getStatusColor(status),
                        fontSize: 12,
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                  ),
                ],
              ),

              const SizedBox(height: 12),

              Text(
                request['description'] ?? '',
                style: const TextStyle(
                  fontSize: 16,
                  fontWeight: FontWeight.w600,
                ),
              ),

              const SizedBox(height: 12),

              Row(
                children: [

                  Icon(
                    Icons.category_outlined,
                    size: 18,
                    color: Colors.grey.shade600,
                  ),

                  const SizedBox(width: 6),

                  Text(
                    request['category'] ?? '',
                    style: TextStyle(
                      color: Colors.grey.shade600,
                    ),
                  ),

                  const Spacer(),

                  Container(
                    padding:
                        const EdgeInsets.symmetric(
                      horizontal: 10,
                      vertical: 5,
                    ),
                    decoration: BoxDecoration(
                      color: getPriorityColor(priority)
                          .withOpacity(0.1),
                      borderRadius:
                          BorderRadius.circular(20),
                    ),
                    child: Text(
                      priority,
                      style: TextStyle(
                        color:
                            getPriorityColor(priority),
                        fontSize: 12,
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                  ),
                ],
              ),
            ],
          ),
        ),
      ),
    );
  }
}