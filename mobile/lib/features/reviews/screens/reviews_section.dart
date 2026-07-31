import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:flutter_animate/flutter_animate.dart';
import '../../../core/theme/app_theme.dart';
import '../providers/reviews_provider.dart';
import 'review_form_screen.dart';

/// Reviews list section shown on the business detail screen.
class ReviewsSection extends ConsumerStatefulWidget {
  final String businessId;
  final String businessName;

  const ReviewsSection({
    super.key,
    required this.businessId,
    required this.businessName,
  });

  @override
  ConsumerState<ReviewsSection> createState() => _ReviewsSectionState();
}

class _ReviewsSectionState extends ConsumerState<ReviewsSection> {
  /// Reviews accumulated across pages so "Load more" appends correctly.
  List<Review> _allReviews = [];
  int _page = 1;
  int _totalPages = 1;
  bool _loaded = false;
  bool _loadingMore = false;
  String? _loadMoreError;

  /// Merge a freshly fetched page into the accumulated list.
  void _mergePage(ReviewPage page, {bool reset = false}) {
    final existing = reset ? <Review>[] : List<Review>.of(_allReviews);
    final existingIds = existing.map((r) => r.id).toSet();
    for (final r in page.items) {
      if (!existingIds.contains(r.id)) existing.add(r);
    }
    _allReviews = existing;
    _page = page.page;
    _totalPages = page.totalPages;
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final colorScheme = theme.colorScheme;
    final reviewsAsync = ref.watch(businessReviewsProvider(widget.businessId));
    final currentUserName = ref.watch(currentUserNameProvider).valueOrNull;

    // Seed the local accumulation once when the first page arrives.
    if (!_loaded && reviewsAsync.hasValue) {
      _mergePage(reviewsAsync.value!, reset: true);
      _loaded = true;
    }

    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Padding(
          padding: const EdgeInsets.fromLTRB(16, 24, 16, 8),
          child: Row(
            children: [
              Expanded(
                child: Text('Reviews',
                    style: theme.textTheme.titleMedium
                        ?.copyWith(fontWeight: FontWeight.w700)),
              ),
              if (currentUserName != null)
                GestureDetector(
                  onTap: _goToWriteReview,
                  child: Container(
                    padding: const EdgeInsets.symmetric(
                        horizontal: 12, vertical: 6),
                    decoration: BoxDecoration(
                      gradient: LinearGradient(
                        colors: [
                          AppTheme.indigoLuxury,
                          const Color(0xFF7C3AED),
                        ],
                        begin: Alignment.topLeft,
                        end: Alignment.bottomRight,
                      ),
                      borderRadius: BorderRadius.circular(AppTheme.radiusFull),
                    ),
                    child: const Row(
                      mainAxisSize: MainAxisSize.min,
                      children: [
                        Icon(Icons.rate_review_outlined,
                            color: Colors.white, size: 14),
                        SizedBox(width: 4),
                        Text('Write a Review',
                            style: TextStyle(
                                color: Colors.white,
                                fontSize: 12,
                                fontWeight: FontWeight.w600)),
                      ],
                    ),
                  ),
                ),
            ],
          ),
        ).animate().fadeIn(duration: 400.ms),

        reviewsAsync.when(
          loading: () => const Padding(
            padding: EdgeInsets.all(32),
            child: Center(child: CircularProgressIndicator()),
          ),
          error: (err, _) => Padding(
            padding: const EdgeInsets.all(24),
            child: Center(
              child: Column(
                mainAxisSize: MainAxisSize.min,
                children: [
                  Icon(Icons.error_outline, size: 40, color: colorScheme.error),
                  const SizedBox(height: 8),
                  Text('Could not load reviews',
                      style: theme.textTheme.titleSmall),
                  const SizedBox(height: 12),
                  FilledButton.tonal(
                    onPressed: () => ref
                        .invalidate(businessReviewsProvider(widget.businessId)),
                    child: const Text('Retry'),
                  ),
                ],
              ),
            ),
          ),
          data: (page) {
            if (_allReviews.isEmpty) {
              return _EmptyReviews(
                hasAccount: currentUserName != null,
                onWrite: currentUserName != null ? _goToWriteReview : null,
              );
            }
            final reviews = _allReviews;
            return Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                ...reviews.asMap().entries.map((entry) {
                  final index = entry.key;
                  final review = entry.value;
                  final isOwn =
                      currentUserName != null && currentUserName == review.customerName;
                  return Padding(
                    padding:
                        const EdgeInsets.only(left: 16, right: 16, bottom: 12),
                    child: GlassContainer(
                      borderRadius: AppTheme.radiusLg,
                      padding: const EdgeInsets.all(16),
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Row(
                            children: [
                              CircleAvatar(
                                radius: 18,
                                backgroundImage: review.customerAvatarUrl != null
                                    ? NetworkImage(review.customerAvatarUrl!)
                                    : null,
                                backgroundColor: colorScheme.surfaceContainerHighest,
                                child: review.customerAvatarUrl == null
                                    ? const Icon(Icons.person, size: 18)
                                    : null,
                              ),
                              const SizedBox(width: 10),
                              Expanded(
                                child: Column(
                                  crossAxisAlignment: CrossAxisAlignment.start,
                                  children: [
                                    Text(
                                      review.customerName,
                                      style: theme.textTheme.titleSmall
                                          ?.copyWith(fontWeight: FontWeight.w600),
                                    ),
                                    if (review.isVerifiedPurchase)
                                      Text(
                                        'Verified purchase',
                                        style: theme.textTheme.labelSmall
                                            ?.copyWith(
                                                color: AppTheme.success,
                                                fontSize: 10),
                                      ),
                                  ],
                                ),
                              ),
                              _StarRating(rating: review.rating, size: 16),
                            ],
                          ),
                          if (review.comment != null &&
                              review.comment!.isNotEmpty) ...[
                            const SizedBox(height: 10),
                            Text(
                              review.comment!,
                              style: theme.textTheme.bodyMedium
                                  ?.copyWith(height: 1.5),
                            ),
                          ],
                          const SizedBox(height: 8),
                          Row(
                            children: [
                              Text(
                                _formatDate(review.createdAt),
                                style: theme.textTheme.labelSmall?.copyWith(
                                    color: colorScheme.onSurfaceVariant),
                              ),
                              const Spacer(),
                              if (isOwn) ...[
                                _ActionChip(
                                  icon: Icons.edit_outlined,
                                  label: 'Edit',
                                  onTap: () => _openReviewForm(review),
                                ),
                                const SizedBox(width: 8),
                                _ActionChip(
                                  icon: Icons.delete_outline,
                                  label: 'Delete',
                                  onTap: () => _confirmDelete(review),
                                ),
                              ],
                            ],
                          ),
                        ],
                      ),
                    ),
                  ).animate().fadeIn(
                      duration: 300.ms, delay: (50 + index * 60).ms);
                }),
                if (_loadingMore)
                  const Padding(
                    padding: EdgeInsets.all(12),
                    child: Center(
                        child: SizedBox(
                            width: 22,
                            height: 22,
                            child: CircularProgressIndicator(strokeWidth: 2))),
                  )
                else if (_loadMoreError != null)
                  Padding(
                    padding: const EdgeInsets.all(12),
                    child: Center(
                      child: Text(_loadMoreError!,
                          style: theme.textTheme.bodySmall
                              ?.copyWith(color: colorScheme.error)),
                    ),
                  )
                else if (_page < _totalPages)
                  Center(
                    child: TextButton.icon(
                      onPressed: _loadMore,
                      icon: const Icon(Icons.expand_more),
                      label: const Text('Load more reviews'),
                    ),
                  ),
              ],
            );
          },
        ),
      ],
    );
  }

  Future<void> _loadMore() async {
    setState(() {
      _loadingMore = true;
      _loadMoreError = null;
    });
    try {
      final api = ref.read(reviewsApiProvider);
      final next = await api.getBusinessReviews(widget.businessId,
          page: _page + 1);
      if (!mounted) return;
      setState(() {
        _mergePage(next);
        _loadingMore = false;
      });
    } catch (_) {
      if (!mounted) return;
      setState(() {
        _loadingMore = false;
        _loadMoreError = 'Could not load more reviews.';
      });
    }
  }  /// Creating a review requires a completed appointment, so the "write"
  /// action routes the user to My Appointments where completed bookings
  /// expose the review form. On return, refresh the list in case a new
  /// review was created.
  void _goToWriteReview() async {
    await context.push('/appointments');
    if (!mounted) return;
    _resetLocal();
    ref.invalidate(businessReviewsProvider(widget.businessId));
  }

  Future<void> _openReviewForm(Review review) async {
    final result = await Navigator.of(context).push<bool>(
      MaterialPageRoute(
        builder: (_) => ReviewFormScreen(
          reviewId: review.id,
          initialRating: review.rating,
          initialComment: review.comment,
          businessName: widget.businessName,
        ),
      ),
    );
    if (result == true) {
      _resetLocal();
      ref.invalidate(businessReviewsProvider(widget.businessId));
    }
  }

  /// Clear locally accumulated reviews so the next provider value re-seeds
  /// from page 1 (e.g. after an edit/delete).
  void _resetLocal() {
    _allReviews = [];
    _page = 1;
    _totalPages = 1;
    _loaded = false;
  }

  Future<void> _confirmDelete(Review review) async {
    final confirmed = await showDialog<bool>(
      context: context,
      builder: (ctx) => AlertDialog(
        title: const Text('Delete review?'),
        content: const Text('This will permanently remove your review.'),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(ctx, false),
            child: const Text('Cancel'),
          ),
          FilledButton(
            style: FilledButton.styleFrom(backgroundColor: Colors.red),
            onPressed: () => Navigator.pop(ctx, true),
            child: const Text('Delete'),
          ),
        ],
      ),
    );
    if (confirmed != true) return;
    try {
      await ref.read(reviewsApiProvider).deleteReview(review.id);
      if (!mounted) return;
      _resetLocal();
      ref.invalidate(businessReviewsProvider(widget.businessId));
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Review deleted.')),
      );
    } catch (_) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Could not delete review.')),
      );
    }
  }

  String _formatDate(DateTime d) {
    const months = [
      'Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun',
      'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec',
    ];
    return '${d.day} ${months[d.month - 1]} ${d.year}';
  }
}

/// Compact star rating display.
class _StarRating extends StatelessWidget {
  final int rating;
  final double size;
  const _StarRating({required this.rating, required this.size});

  @override
  Widget build(BuildContext context) {
    return Row(
      mainAxisSize: MainAxisSize.min,
      children: List.generate(5, (i) {
        return Icon(
          i < rating ? Icons.star_rounded : Icons.star_outline_rounded,
          size: size,
          color: i < rating ? const Color(0xFFF59E0B) : Colors.grey.shade400,
        );
      }),
    );
  }
}

class _ActionChip extends StatelessWidget {
  final IconData icon;
  final String label;
  final VoidCallback onTap;
  const _ActionChip({
    required this.icon,
    required this.label,
    required this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTap: onTap,
      child: Container(
        padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
        decoration: BoxDecoration(
          color: AppTheme.indigoLuxury.withValues(alpha: 0.1),
          borderRadius: BorderRadius.circular(AppTheme.radiusFull),
        ),
        child: Row(
          mainAxisSize: MainAxisSize.min,
          children: [
            Icon(icon, size: 13, color: AppTheme.indigoLuxury),
            const SizedBox(width: 3),
            Text(label,
                style: const TextStyle(
                    color: AppTheme.indigoLuxury,
                    fontSize: 11,
                    fontWeight: FontWeight.w600)),
          ],
        ),
      ),
    );
  }
}

class _EmptyReviews extends StatelessWidget {
  final bool hasAccount;
  final VoidCallback? onWrite;
  const _EmptyReviews({required this.hasAccount, this.onWrite});

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final colorScheme = theme.colorScheme;
    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 16),
      child: GlassContainer(
        borderRadius: AppTheme.radiusLg,
        padding: const EdgeInsets.all(24),
        child: Column(
          children: [
            Icon(Icons.rate_review_outlined,
                size: 40, color: colorScheme.onSurfaceVariant.withValues(alpha: 0.5)),
            const SizedBox(height: 12),
            Text('No reviews yet',
                style: theme.textTheme.titleSmall
                    ?.copyWith(fontWeight: FontWeight.w600)),
            const SizedBox(height: 4),
            Text(
              hasAccount
                  ? 'Be the first to share your experience!'
                  : 'Reviews will appear here once customers share their experience.',
              textAlign: TextAlign.center,
              style: theme.textTheme.bodySmall
                  ?.copyWith(color: colorScheme.onSurfaceVariant),
            ),
            if (hasAccount && onWrite != null) ...[
              const SizedBox(height: 16),
              SizedBox(
                height: 40,
                child: DecoratedBox(
                  decoration: BoxDecoration(
                    gradient: LinearGradient(
                      colors: [AppTheme.indigoLuxury, const Color(0xFF7C3AED)],
                      begin: Alignment.topLeft,
                      end: Alignment.bottomRight,
                    ),
                    borderRadius: BorderRadius.circular(AppTheme.radiusFull),
                  ),
                  child: MaterialButton(
                    onPressed: onWrite,
                    shape: RoundedRectangleBorder(
                        borderRadius:
                            BorderRadius.circular(AppTheme.radiusFull)),
                    child: const Text('Write a Review',
                        style: TextStyle(
                            color: Colors.white,
                            fontSize: 13,
                            fontWeight: FontWeight.w600)),
                  ),
                ),
              ),
            ],
          ],
        ),
      ),
    );
  }
}
