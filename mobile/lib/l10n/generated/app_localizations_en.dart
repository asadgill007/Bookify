// ignore: unused_import
import 'package:intl/intl.dart' as intl;
import 'app_localizations.dart';

// ignore_for_file: type=lint

/// The translations for English (`en`).
class AppLocalizationsEn extends AppLocalizations {
  AppLocalizationsEn([String locale = 'en']) : super(locale);

  @override
  String get appTitle => 'Bookify';

  @override
  String get appTagline => 'Find your perfect service';

  @override
  String get appSubtitle =>
      'AI Powered Global Multi-Service Appointment Booking Platform';

  @override
  String get navHome => 'Home';

  @override
  String get navSearch => 'Search';

  @override
  String get navAppointments => 'Appointments';

  @override
  String get navProfile => 'Profile';

  @override
  String get commonCancel => 'Cancel';

  @override
  String get commonSave => 'Save';

  @override
  String get commonDelete => 'Delete';

  @override
  String get commonRetry => 'Retry';

  @override
  String get commonClose => 'Close';

  @override
  String get commonSubmit => 'Submit';

  @override
  String get commonSend => 'Send';

  @override
  String get commonSearch => 'Search';

  @override
  String get commonLoading => 'Loading...';

  @override
  String get commonError => 'Something went wrong';

  @override
  String get commonConfirm => 'Confirm';

  @override
  String get commonBack => 'Back';

  @override
  String get commonYes => 'Yes';

  @override
  String get commonNo => 'No';

  @override
  String get commonNext => 'Next';

  @override
  String get commonOptional => '(optional)';

  @override
  String get commonNoResults => 'No results found';

  @override
  String get commonViewAll => 'See All';

  @override
  String get commonSettings => 'Settings';

  @override
  String get commonNotifications => 'Notifications';

  @override
  String get commonSupport => 'Support';

  @override
  String get commonLanguage => 'Language';

  @override
  String get commonCurrency => 'Currency';

  @override
  String get commonAppearance => 'Appearance';

  @override
  String get commonAccount => 'Account';

  @override
  String get commonEditProfile => 'Edit Profile';

  @override
  String get commonLogout => 'Sign Out';

  @override
  String get commonVerified => 'Verified';

  @override
  String get homeDiscover => 'Discover';

  @override
  String get homeSearchHint => 'Search for services...';

  @override
  String get homeCategories => 'Categories';

  @override
  String get homeFeatured => 'Featured Businesses';

  @override
  String get homeChatBot => 'Chat with Bookify Assistant';

  @override
  String get searchHint => 'Search businesses...';

  @override
  String get searchEmpty => 'Search for doctors, salons, spas...';

  @override
  String get searchEmptySubtitle =>
      'Try searching by name, category, or location';

  @override
  String get searchFailed => 'Search failed';

  @override
  String searchNoResults(Object query) {
    return 'No results found for \"$query\"';
  }

  @override
  String get searchNoResultsSubtitle => 'Try a different search term';

  @override
  String get searchFilterByCategory => 'Filter by category';

  @override
  String get searchNoCategories => 'No categories available';

  @override
  String get searchFilters => 'Filters';

  @override
  String get searchFilterPriceRange => 'Price range';

  @override
  String get searchFilterDistance => 'Distance radius';

  @override
  String get searchFilterRating => 'Minimum rating';

  @override
  String get searchFilterCategory => 'Category';

  @override
  String get searchFilterClear => 'Clear filters';

  @override
  String get searchFilterApply => 'Apply';

  @override
  String get searchAnyPrice => 'Any price';

  @override
  String get searchAnyDistance => 'Anywhere';

  @override
  String get searchAnyRating => 'Any rating';

  @override
  String get searchActiveFilters => 'Active filters';

  @override
  String searchKm(Object km) {
    return '$km km';
  }

  @override
  String searchPriceRangeHint(Object max, Object min) {
    return '$min – $max';
  }

  @override
  String searchStars(Object rating) {
    return '$rating★';
  }

  @override
  String get profileTitle => 'Profile';

  @override
  String get profileBookings => 'Bookings';

  @override
  String get profileEditProfile => 'Edit Profile';

  @override
  String get profileMyAppointments => 'My Appointments';

  @override
  String get profileMyWaitlist => 'My Waitlist';

  @override
  String get profileRecurring => 'Recurring Bookings';

  @override
  String get profileMyBusiness => 'My Business';

  @override
  String get profileReviewBusinesses => 'Review Businesses';

  @override
  String get profileFavorites => 'Favorites';

  @override
  String get profileHelp => 'Help Center';

  @override
  String get profileAbout => 'About';

  @override
  String get profileContactSupport => 'Contact Support';

  @override
  String get profileReportProblem => 'Report a Problem';

  @override
  String get profileTerms => 'Terms of Service';

  @override
  String get profilePrivacy => 'Privacy Policy';

  @override
  String get settingsTitle => 'Settings';

  @override
  String get settingsDarkMode => 'Dark Mode';

  @override
  String get settingsSystem => 'System';

  @override
  String get settingsLight => 'Light';

  @override
  String get settingsDark => 'Dark';

  @override
  String get settingsLanguage => 'Language';

  @override
  String get settingsCurrency => 'Currency';

  @override
  String get settingsNotifications => 'Notifications';

  @override
  String get settingsPushNotifications => 'Push Notifications';

  @override
  String get settingsPushSubtitle => 'Receive booking updates';

  @override
  String get settingsEmailNotifications => 'Email Notifications';

  @override
  String get settingsEmailSubtitle => 'Receive promotional emails';

  @override
  String get settingsChangePassword => 'Change Password';

  @override
  String get settingsDeleteAccount => 'Delete Account';

  @override
  String get settingsDeleteAccountSubtitle =>
      'This permanently deletes your account and all data. This cannot be undone.';

  @override
  String get settingsDeleting => 'Deleting account...';

  @override
  String get settingsSelectLanguage => 'Select Language';

  @override
  String get settingsSelectCurrency => 'Select Currency';

  @override
  String get authWelcomeBack => 'Welcome Back';

  @override
  String get authSignInSubtitle => 'Sign in to manage your appointments';

  @override
  String get authEmail => 'Email';

  @override
  String get authPassword => 'Password';

  @override
  String get authSignIn => 'Sign In';

  @override
  String get authForgotPassword => 'Forgot Password?';

  @override
  String get authNoAccount => 'Don\'t have an account? ';

  @override
  String get authCreateAccount => 'Create Account';

  @override
  String get authSignInWithGoogle => 'Sign in with Google';

  @override
  String get authEmailRequired => 'Email is required';

  @override
  String get authValidEmail => 'Enter a valid email';

  @override
  String get authPasswordRequired => 'Password is required';

  @override
  String get authPasswordMin => 'Password must be at least 6 characters';

  @override
  String get authCreateSubtitle =>
      'Join Bookify and start booking premium services';

  @override
  String get authFirstName => 'First Name';

  @override
  String get authLastName => 'Last Name';

  @override
  String get authConfirmPassword => 'Confirm Password';

  @override
  String get authRequired => 'Required';

  @override
  String get authAtLeast8 => 'At least 8 characters';

  @override
  String get authPasswordsMatch => 'Passwords do not match';

  @override
  String get authAlreadyAccount => 'Already have an account? ';

  @override
  String get authCustomer => 'Customer';

  @override
  String get authCustomerSubtitle => 'Book services near you';

  @override
  String get authListBusiness => 'List your business';

  @override
  String get authListBusinessSubtitle =>
      'Owners & staff join to manage bookings';

  @override
  String get authGoogleError =>
      'Google sign-in is not configured for this build. Add a web client ID via --dart-define=GOOGLE_CLIENT_ID.';

  @override
  String get businessBookNow => 'Book Now';

  @override
  String get businessAbout => 'About';

  @override
  String get businessGallery => 'Gallery';

  @override
  String get businessServices => 'Services';

  @override
  String get businessOurTeam => 'Our Team';

  @override
  String get businessOpeningHours => 'Opening Hours';

  @override
  String get businessClosed => 'Closed';

  @override
  String get businessNoStaff => 'No staff';

  @override
  String get businessBook => 'Book';

  @override
  String businessMinutes(Object minutes) {
    return '$minutes min';
  }

  @override
  String businessReviews(Object count) {
    return '($count reviews)';
  }

  @override
  String get favoritesTitle => 'My Favorites';

  @override
  String get favoritesEmpty => 'No favorites yet';

  @override
  String get favoritesEmptySubtitle =>
      'Tap the heart on any business to save it here';

  @override
  String get favoritesAdd => 'Added to favorites';

  @override
  String get favoritesRemove => 'Removed from favorites';

  @override
  String get chatTitle => 'Bookify Assistant';

  @override
  String get chatHint => 'Ask me anything...';

  @override
  String get chatGreeting =>
      'Hello! I\'m Bookify\'s assistant. Ask me to find a business, check your booking, or answer a question.';

  @override
  String get chatSuggestFind => 'Find a salon near me';

  @override
  String get chatSuggestBooking => 'Check my booking status';

  @override
  String get chatSuggestCancel => 'How do I cancel a booking?';

  @override
  String get chatSuggested => 'Try asking:';

  @override
  String get chatError => 'Could not send your message. Please try again.';

  @override
  String get supportHelpCenter => 'Help Center';

  @override
  String get supportContact => 'Contact Support';

  @override
  String get supportReportProblem => 'Report a Problem';

  @override
  String get supportSubject => 'Subject';

  @override
  String get supportMessage => 'Message';

  @override
  String get supportCategory => 'Category';

  @override
  String get supportCategoryGeneral => 'General';

  @override
  String get supportCategoryBooking => 'Booking issue';

  @override
  String get supportCategoryPayment => 'Payment';

  @override
  String get supportCategoryCancellation => 'Cancellation';

  @override
  String get supportCategoryAccount => 'Account';

  @override
  String get supportCategoryProvider => 'Provider question';

  @override
  String get supportSubjectHint => 'Brief summary of the issue';

  @override
  String get supportMessageHint => 'Tell us what happened...';

  @override
  String get supportContactEmail => 'Contact email (optional)';

  @override
  String get supportSubmitted =>
      'Ticket submitted. We\'ll get back to you soon.';

  @override
  String get supportFailed => 'Could not submit. Please try again.';

  @override
  String get supportReportAppointment =>
      'Report an issue with this appointment';

  @override
  String get supportReportHint =>
      'Describe what went wrong with this appointment';

  @override
  String get myBusinessTitle => 'My Business';

  @override
  String get myBusinessListNew => 'List a new business';

  @override
  String get myBusinessEmpty => 'You have no businesses yet';

  @override
  String get myBusinessEmptySubtitle =>
      'List your business and start receiving bookings in minutes.';

  @override
  String get myBusinessListYour => 'List Your Business';

  @override
  String myBusinessServices(Object count) {
    return '$count services';
  }

  @override
  String myBusinessStaff(Object count) {
    return '$count staff';
  }

  @override
  String get myBusinessResubmit => 'Resubmit for Review';

  @override
  String get myBusinessViewListing => 'View Listing';

  @override
  String get myBusinessAwaitingReview => 'Awaiting review';

  @override
  String get myBusinessLive =>
      'Your business is live and visible to customers.';

  @override
  String get myBusinessPending =>
      'Complete the checklist below and your business goes live automatically.';

  @override
  String get myBusinessRejected =>
      'Your listing was rejected by our review team.';

  @override
  String get myBusinessChecklist => 'To go live, complete these steps:';

  @override
  String get myBusinessPreview => 'Preview how customers see it';

  @override
  String get myBusinessStatusApproved => 'Live';

  @override
  String get myBusinessStatusPending => 'In progress';

  @override
  String get myBusinessStatusRejected => 'Rejected';

  @override
  String get myBusinessAutoNotice =>
      'No admin approval needed — once the checklist is complete your business goes live automatically.';

  @override
  String get onboardingTitle => 'Welcome to Bookify';

  @override
  String get onboardingSubtitle =>
      'Create your business listing and start receiving bookings.';
}
