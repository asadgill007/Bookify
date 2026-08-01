import 'dart:async';

import 'package:flutter/foundation.dart';
import 'package:flutter/widgets.dart';
import 'package:flutter_localizations/flutter_localizations.dart';
import 'package:intl/intl.dart' as intl;

import 'app_localizations_en.dart';
import 'app_localizations_ur.dart';

// ignore_for_file: type=lint

/// Callers can lookup localized strings with an instance of AppLocalizations
/// returned by `AppLocalizations.of(context)`.
///
/// Applications need to include `AppLocalizations.delegate()` in their app's
/// `localizationDelegates` list, and the locales they support in the app's
/// `supportedLocales` list. For example:
///
/// ```dart
/// import 'generated/app_localizations.dart';
///
/// return MaterialApp(
///   localizationsDelegates: AppLocalizations.localizationsDelegates,
///   supportedLocales: AppLocalizations.supportedLocales,
///   home: MyApplicationHome(),
/// );
/// ```
///
/// ## Update pubspec.yaml
///
/// Please make sure to update your pubspec.yaml to include the following
/// packages:
///
/// ```yaml
/// dependencies:
///   # Internationalization support.
///   flutter_localizations:
///     sdk: flutter
///   intl: any # Use the pinned version from flutter_localizations
///
///   # Rest of dependencies
/// ```
///
/// ## iOS Applications
///
/// iOS applications define key application metadata, including supported
/// locales, in an Info.plist file that is built into the application bundle.
/// To configure the locales supported by your app, you’ll need to edit this
/// file.
///
/// First, open your project’s ios/Runner.xcworkspace Xcode workspace file.
/// Then, in the Project Navigator, open the Info.plist file under the Runner
/// project’s Runner folder.
///
/// Next, select the Information Property List item, select Add Item from the
/// Editor menu, then select Localizations from the pop-up menu.
///
/// Select and expand the newly-created Localizations item then, for each
/// locale your application supports, add a new item and select the locale
/// you wish to add from the pop-up menu in the Value field. This list should
/// be consistent with the languages listed in the AppLocalizations.supportedLocales
/// property.
abstract class AppLocalizations {
  AppLocalizations(String locale)
    : localeName = intl.Intl.canonicalizedLocale(locale.toString());

  final String localeName;

  static AppLocalizations of(BuildContext context) {
    return Localizations.of<AppLocalizations>(context, AppLocalizations)!;
  }

  static const LocalizationsDelegate<AppLocalizations> delegate =
      _AppLocalizationsDelegate();

  /// A list of this localizations delegate along with the default localizations
  /// delegates.
  ///
  /// Returns a list of localizations delegates containing this delegate along with
  /// GlobalMaterialLocalizations.delegate, GlobalCupertinoLocalizations.delegate,
  /// and GlobalWidgetsLocalizations.delegate.
  ///
  /// Additional delegates can be added by appending to this list in
  /// MaterialApp. This list does not have to be used at all if a custom list
  /// of delegates is preferred or required.
  static const List<LocalizationsDelegate<dynamic>> localizationsDelegates =
      <LocalizationsDelegate<dynamic>>[
        delegate,
        GlobalMaterialLocalizations.delegate,
        GlobalCupertinoLocalizations.delegate,
        GlobalWidgetsLocalizations.delegate,
      ];

  /// A list of this localizations delegate's supported locales.
  static const List<Locale> supportedLocales = <Locale>[
    Locale('en'),
    Locale('ur'),
  ];

  /// No description provided for @appTitle.
  ///
  /// In en, this message translates to:
  /// **'Bookify'**
  String get appTitle;

  /// No description provided for @appTagline.
  ///
  /// In en, this message translates to:
  /// **'Find your perfect service'**
  String get appTagline;

  /// No description provided for @appSubtitle.
  ///
  /// In en, this message translates to:
  /// **'AI Powered Global Multi-Service Appointment Booking Platform'**
  String get appSubtitle;

  /// No description provided for @navHome.
  ///
  /// In en, this message translates to:
  /// **'Home'**
  String get navHome;

  /// No description provided for @navSearch.
  ///
  /// In en, this message translates to:
  /// **'Search'**
  String get navSearch;

  /// No description provided for @navAppointments.
  ///
  /// In en, this message translates to:
  /// **'Appointments'**
  String get navAppointments;

  /// No description provided for @navProfile.
  ///
  /// In en, this message translates to:
  /// **'Profile'**
  String get navProfile;

  /// No description provided for @commonCancel.
  ///
  /// In en, this message translates to:
  /// **'Cancel'**
  String get commonCancel;

  /// No description provided for @commonSave.
  ///
  /// In en, this message translates to:
  /// **'Save'**
  String get commonSave;

  /// No description provided for @commonDelete.
  ///
  /// In en, this message translates to:
  /// **'Delete'**
  String get commonDelete;

  /// No description provided for @commonRetry.
  ///
  /// In en, this message translates to:
  /// **'Retry'**
  String get commonRetry;

  /// No description provided for @commonClose.
  ///
  /// In en, this message translates to:
  /// **'Close'**
  String get commonClose;

  /// No description provided for @commonSubmit.
  ///
  /// In en, this message translates to:
  /// **'Submit'**
  String get commonSubmit;

  /// No description provided for @commonSend.
  ///
  /// In en, this message translates to:
  /// **'Send'**
  String get commonSend;

  /// No description provided for @commonSearch.
  ///
  /// In en, this message translates to:
  /// **'Search'**
  String get commonSearch;

  /// No description provided for @commonLoading.
  ///
  /// In en, this message translates to:
  /// **'Loading...'**
  String get commonLoading;

  /// No description provided for @commonError.
  ///
  /// In en, this message translates to:
  /// **'Something went wrong'**
  String get commonError;

  /// No description provided for @commonConfirm.
  ///
  /// In en, this message translates to:
  /// **'Confirm'**
  String get commonConfirm;

  /// No description provided for @commonBack.
  ///
  /// In en, this message translates to:
  /// **'Back'**
  String get commonBack;

  /// No description provided for @commonYes.
  ///
  /// In en, this message translates to:
  /// **'Yes'**
  String get commonYes;

  /// No description provided for @commonNo.
  ///
  /// In en, this message translates to:
  /// **'No'**
  String get commonNo;

  /// No description provided for @commonNext.
  ///
  /// In en, this message translates to:
  /// **'Next'**
  String get commonNext;

  /// No description provided for @commonOptional.
  ///
  /// In en, this message translates to:
  /// **'(optional)'**
  String get commonOptional;

  /// No description provided for @commonNoResults.
  ///
  /// In en, this message translates to:
  /// **'No results found'**
  String get commonNoResults;

  /// No description provided for @commonViewAll.
  ///
  /// In en, this message translates to:
  /// **'See All'**
  String get commonViewAll;

  /// No description provided for @commonSettings.
  ///
  /// In en, this message translates to:
  /// **'Settings'**
  String get commonSettings;

  /// No description provided for @commonNotifications.
  ///
  /// In en, this message translates to:
  /// **'Notifications'**
  String get commonNotifications;

  /// No description provided for @commonSupport.
  ///
  /// In en, this message translates to:
  /// **'Support'**
  String get commonSupport;

  /// No description provided for @commonLanguage.
  ///
  /// In en, this message translates to:
  /// **'Language'**
  String get commonLanguage;

  /// No description provided for @commonCurrency.
  ///
  /// In en, this message translates to:
  /// **'Currency'**
  String get commonCurrency;

  /// No description provided for @commonAppearance.
  ///
  /// In en, this message translates to:
  /// **'Appearance'**
  String get commonAppearance;

  /// No description provided for @commonAccount.
  ///
  /// In en, this message translates to:
  /// **'Account'**
  String get commonAccount;

  /// No description provided for @commonEditProfile.
  ///
  /// In en, this message translates to:
  /// **'Edit Profile'**
  String get commonEditProfile;

  /// No description provided for @commonLogout.
  ///
  /// In en, this message translates to:
  /// **'Sign Out'**
  String get commonLogout;

  /// No description provided for @commonVerified.
  ///
  /// In en, this message translates to:
  /// **'Verified'**
  String get commonVerified;

  /// No description provided for @homeDiscover.
  ///
  /// In en, this message translates to:
  /// **'Discover'**
  String get homeDiscover;

  /// No description provided for @homeSearchHint.
  ///
  /// In en, this message translates to:
  /// **'Search for services...'**
  String get homeSearchHint;

  /// No description provided for @homeCategories.
  ///
  /// In en, this message translates to:
  /// **'Categories'**
  String get homeCategories;

  /// No description provided for @homeFeatured.
  ///
  /// In en, this message translates to:
  /// **'Featured Businesses'**
  String get homeFeatured;

  /// No description provided for @homeChatBot.
  ///
  /// In en, this message translates to:
  /// **'Chat with Bookify Assistant'**
  String get homeChatBot;

  /// No description provided for @searchHint.
  ///
  /// In en, this message translates to:
  /// **'Search businesses...'**
  String get searchHint;

  /// No description provided for @searchEmpty.
  ///
  /// In en, this message translates to:
  /// **'Search for doctors, salons, spas...'**
  String get searchEmpty;

  /// No description provided for @searchEmptySubtitle.
  ///
  /// In en, this message translates to:
  /// **'Try searching by name, category, or location'**
  String get searchEmptySubtitle;

  /// No description provided for @searchFailed.
  ///
  /// In en, this message translates to:
  /// **'Search failed'**
  String get searchFailed;

  /// No description provided for @searchNoResults.
  ///
  /// In en, this message translates to:
  /// **'No results found for \"{query}\"'**
  String searchNoResults(Object query);

  /// No description provided for @searchNoResultsSubtitle.
  ///
  /// In en, this message translates to:
  /// **'Try a different search term'**
  String get searchNoResultsSubtitle;

  /// No description provided for @searchFilterByCategory.
  ///
  /// In en, this message translates to:
  /// **'Filter by category'**
  String get searchFilterByCategory;

  /// No description provided for @searchNoCategories.
  ///
  /// In en, this message translates to:
  /// **'No categories available'**
  String get searchNoCategories;

  /// No description provided for @searchFilters.
  ///
  /// In en, this message translates to:
  /// **'Filters'**
  String get searchFilters;

  /// No description provided for @searchFilterPriceRange.
  ///
  /// In en, this message translates to:
  /// **'Price range'**
  String get searchFilterPriceRange;

  /// No description provided for @searchFilterDistance.
  ///
  /// In en, this message translates to:
  /// **'Distance radius'**
  String get searchFilterDistance;

  /// No description provided for @searchFilterRating.
  ///
  /// In en, this message translates to:
  /// **'Minimum rating'**
  String get searchFilterRating;

  /// No description provided for @searchFilterCategory.
  ///
  /// In en, this message translates to:
  /// **'Category'**
  String get searchFilterCategory;

  /// No description provided for @searchFilterClear.
  ///
  /// In en, this message translates to:
  /// **'Clear filters'**
  String get searchFilterClear;

  /// No description provided for @searchFilterApply.
  ///
  /// In en, this message translates to:
  /// **'Apply'**
  String get searchFilterApply;

  /// No description provided for @searchAnyPrice.
  ///
  /// In en, this message translates to:
  /// **'Any price'**
  String get searchAnyPrice;

  /// No description provided for @searchAnyDistance.
  ///
  /// In en, this message translates to:
  /// **'Anywhere'**
  String get searchAnyDistance;

  /// No description provided for @searchAnyRating.
  ///
  /// In en, this message translates to:
  /// **'Any rating'**
  String get searchAnyRating;

  /// No description provided for @searchActiveFilters.
  ///
  /// In en, this message translates to:
  /// **'Active filters'**
  String get searchActiveFilters;

  /// No description provided for @searchKm.
  ///
  /// In en, this message translates to:
  /// **'{km} km'**
  String searchKm(Object km);

  /// No description provided for @searchPriceRangeHint.
  ///
  /// In en, this message translates to:
  /// **'{min} – {max}'**
  String searchPriceRangeHint(Object max, Object min);

  /// No description provided for @searchStars.
  ///
  /// In en, this message translates to:
  /// **'{rating}★'**
  String searchStars(Object rating);

  /// No description provided for @profileTitle.
  ///
  /// In en, this message translates to:
  /// **'Profile'**
  String get profileTitle;

  /// No description provided for @profileBookings.
  ///
  /// In en, this message translates to:
  /// **'Bookings'**
  String get profileBookings;

  /// No description provided for @profileEditProfile.
  ///
  /// In en, this message translates to:
  /// **'Edit Profile'**
  String get profileEditProfile;

  /// No description provided for @profileMyAppointments.
  ///
  /// In en, this message translates to:
  /// **'My Appointments'**
  String get profileMyAppointments;

  /// No description provided for @profileMyWaitlist.
  ///
  /// In en, this message translates to:
  /// **'My Waitlist'**
  String get profileMyWaitlist;

  /// No description provided for @profileRecurring.
  ///
  /// In en, this message translates to:
  /// **'Recurring Bookings'**
  String get profileRecurring;

  /// No description provided for @profileMyBusiness.
  ///
  /// In en, this message translates to:
  /// **'My Business'**
  String get profileMyBusiness;

  /// No description provided for @profileReviewBusinesses.
  ///
  /// In en, this message translates to:
  /// **'Review Businesses'**
  String get profileReviewBusinesses;

  /// No description provided for @profileFavorites.
  ///
  /// In en, this message translates to:
  /// **'Favorites'**
  String get profileFavorites;

  /// No description provided for @profileHelp.
  ///
  /// In en, this message translates to:
  /// **'Help Center'**
  String get profileHelp;

  /// No description provided for @profileAbout.
  ///
  /// In en, this message translates to:
  /// **'About'**
  String get profileAbout;

  /// No description provided for @profileContactSupport.
  ///
  /// In en, this message translates to:
  /// **'Contact Support'**
  String get profileContactSupport;

  /// No description provided for @profileReportProblem.
  ///
  /// In en, this message translates to:
  /// **'Report a Problem'**
  String get profileReportProblem;

  /// No description provided for @profileTerms.
  ///
  /// In en, this message translates to:
  /// **'Terms of Service'**
  String get profileTerms;

  /// No description provided for @profilePrivacy.
  ///
  /// In en, this message translates to:
  /// **'Privacy Policy'**
  String get profilePrivacy;

  /// No description provided for @settingsTitle.
  ///
  /// In en, this message translates to:
  /// **'Settings'**
  String get settingsTitle;

  /// No description provided for @settingsDarkMode.
  ///
  /// In en, this message translates to:
  /// **'Dark Mode'**
  String get settingsDarkMode;

  /// No description provided for @settingsSystem.
  ///
  /// In en, this message translates to:
  /// **'System'**
  String get settingsSystem;

  /// No description provided for @settingsLight.
  ///
  /// In en, this message translates to:
  /// **'Light'**
  String get settingsLight;

  /// No description provided for @settingsDark.
  ///
  /// In en, this message translates to:
  /// **'Dark'**
  String get settingsDark;

  /// No description provided for @settingsLanguage.
  ///
  /// In en, this message translates to:
  /// **'Language'**
  String get settingsLanguage;

  /// No description provided for @settingsCurrency.
  ///
  /// In en, this message translates to:
  /// **'Currency'**
  String get settingsCurrency;

  /// No description provided for @settingsNotifications.
  ///
  /// In en, this message translates to:
  /// **'Notifications'**
  String get settingsNotifications;

  /// No description provided for @settingsPushNotifications.
  ///
  /// In en, this message translates to:
  /// **'Push Notifications'**
  String get settingsPushNotifications;

  /// No description provided for @settingsPushSubtitle.
  ///
  /// In en, this message translates to:
  /// **'Receive booking updates'**
  String get settingsPushSubtitle;

  /// No description provided for @settingsEmailNotifications.
  ///
  /// In en, this message translates to:
  /// **'Email Notifications'**
  String get settingsEmailNotifications;

  /// No description provided for @settingsEmailSubtitle.
  ///
  /// In en, this message translates to:
  /// **'Receive promotional emails'**
  String get settingsEmailSubtitle;

  /// No description provided for @settingsChangePassword.
  ///
  /// In en, this message translates to:
  /// **'Change Password'**
  String get settingsChangePassword;

  /// No description provided for @settingsDeleteAccount.
  ///
  /// In en, this message translates to:
  /// **'Delete Account'**
  String get settingsDeleteAccount;

  /// No description provided for @settingsDeleteAccountSubtitle.
  ///
  /// In en, this message translates to:
  /// **'This permanently deletes your account and all data. This cannot be undone.'**
  String get settingsDeleteAccountSubtitle;

  /// No description provided for @settingsDeleting.
  ///
  /// In en, this message translates to:
  /// **'Deleting account...'**
  String get settingsDeleting;

  /// No description provided for @settingsSelectLanguage.
  ///
  /// In en, this message translates to:
  /// **'Select Language'**
  String get settingsSelectLanguage;

  /// No description provided for @settingsSelectCurrency.
  ///
  /// In en, this message translates to:
  /// **'Select Currency'**
  String get settingsSelectCurrency;

  /// No description provided for @authWelcomeBack.
  ///
  /// In en, this message translates to:
  /// **'Welcome Back'**
  String get authWelcomeBack;

  /// No description provided for @authSignInSubtitle.
  ///
  /// In en, this message translates to:
  /// **'Sign in to manage your appointments'**
  String get authSignInSubtitle;

  /// No description provided for @authEmail.
  ///
  /// In en, this message translates to:
  /// **'Email'**
  String get authEmail;

  /// No description provided for @authPassword.
  ///
  /// In en, this message translates to:
  /// **'Password'**
  String get authPassword;

  /// No description provided for @authSignIn.
  ///
  /// In en, this message translates to:
  /// **'Sign In'**
  String get authSignIn;

  /// No description provided for @authForgotPassword.
  ///
  /// In en, this message translates to:
  /// **'Forgot Password?'**
  String get authForgotPassword;

  /// No description provided for @authNoAccount.
  ///
  /// In en, this message translates to:
  /// **'Don\'t have an account? '**
  String get authNoAccount;

  /// No description provided for @authCreateAccount.
  ///
  /// In en, this message translates to:
  /// **'Create Account'**
  String get authCreateAccount;

  /// No description provided for @authSignInWithGoogle.
  ///
  /// In en, this message translates to:
  /// **'Sign in with Google'**
  String get authSignInWithGoogle;

  /// No description provided for @authEmailRequired.
  ///
  /// In en, this message translates to:
  /// **'Email is required'**
  String get authEmailRequired;

  /// No description provided for @authValidEmail.
  ///
  /// In en, this message translates to:
  /// **'Enter a valid email'**
  String get authValidEmail;

  /// No description provided for @authPasswordRequired.
  ///
  /// In en, this message translates to:
  /// **'Password is required'**
  String get authPasswordRequired;

  /// No description provided for @authPasswordMin.
  ///
  /// In en, this message translates to:
  /// **'Password must be at least 6 characters'**
  String get authPasswordMin;

  /// No description provided for @authCreateSubtitle.
  ///
  /// In en, this message translates to:
  /// **'Join Bookify and start booking premium services'**
  String get authCreateSubtitle;

  /// No description provided for @authFirstName.
  ///
  /// In en, this message translates to:
  /// **'First Name'**
  String get authFirstName;

  /// No description provided for @authLastName.
  ///
  /// In en, this message translates to:
  /// **'Last Name'**
  String get authLastName;

  /// No description provided for @authConfirmPassword.
  ///
  /// In en, this message translates to:
  /// **'Confirm Password'**
  String get authConfirmPassword;

  /// No description provided for @authRequired.
  ///
  /// In en, this message translates to:
  /// **'Required'**
  String get authRequired;

  /// No description provided for @authAtLeast8.
  ///
  /// In en, this message translates to:
  /// **'At least 8 characters'**
  String get authAtLeast8;

  /// No description provided for @authPasswordsMatch.
  ///
  /// In en, this message translates to:
  /// **'Passwords do not match'**
  String get authPasswordsMatch;

  /// No description provided for @authAlreadyAccount.
  ///
  /// In en, this message translates to:
  /// **'Already have an account? '**
  String get authAlreadyAccount;

  /// No description provided for @authCustomer.
  ///
  /// In en, this message translates to:
  /// **'Customer'**
  String get authCustomer;

  /// No description provided for @authCustomerSubtitle.
  ///
  /// In en, this message translates to:
  /// **'Book services near you'**
  String get authCustomerSubtitle;

  /// No description provided for @authListBusiness.
  ///
  /// In en, this message translates to:
  /// **'List your business'**
  String get authListBusiness;

  /// No description provided for @authListBusinessSubtitle.
  ///
  /// In en, this message translates to:
  /// **'Owners & staff join to manage bookings'**
  String get authListBusinessSubtitle;

  /// No description provided for @authGoogleError.
  ///
  /// In en, this message translates to:
  /// **'Google sign-in is not configured for this build. Add a web client ID via --dart-define=GOOGLE_CLIENT_ID.'**
  String get authGoogleError;

  /// No description provided for @businessBookNow.
  ///
  /// In en, this message translates to:
  /// **'Book Now'**
  String get businessBookNow;

  /// No description provided for @businessAbout.
  ///
  /// In en, this message translates to:
  /// **'About'**
  String get businessAbout;

  /// No description provided for @businessGallery.
  ///
  /// In en, this message translates to:
  /// **'Gallery'**
  String get businessGallery;

  /// No description provided for @businessServices.
  ///
  /// In en, this message translates to:
  /// **'Services'**
  String get businessServices;

  /// No description provided for @businessOurTeam.
  ///
  /// In en, this message translates to:
  /// **'Our Team'**
  String get businessOurTeam;

  /// No description provided for @businessOpeningHours.
  ///
  /// In en, this message translates to:
  /// **'Opening Hours'**
  String get businessOpeningHours;

  /// No description provided for @businessClosed.
  ///
  /// In en, this message translates to:
  /// **'Closed'**
  String get businessClosed;

  /// No description provided for @businessNoStaff.
  ///
  /// In en, this message translates to:
  /// **'No staff'**
  String get businessNoStaff;

  /// No description provided for @businessBook.
  ///
  /// In en, this message translates to:
  /// **'Book'**
  String get businessBook;

  /// No description provided for @businessMinutes.
  ///
  /// In en, this message translates to:
  /// **'{minutes} min'**
  String businessMinutes(Object minutes);

  /// No description provided for @businessReviews.
  ///
  /// In en, this message translates to:
  /// **'({count} reviews)'**
  String businessReviews(Object count);

  /// No description provided for @favoritesTitle.
  ///
  /// In en, this message translates to:
  /// **'My Favorites'**
  String get favoritesTitle;

  /// No description provided for @favoritesEmpty.
  ///
  /// In en, this message translates to:
  /// **'No favorites yet'**
  String get favoritesEmpty;

  /// No description provided for @favoritesEmptySubtitle.
  ///
  /// In en, this message translates to:
  /// **'Tap the heart on any business to save it here'**
  String get favoritesEmptySubtitle;

  /// No description provided for @favoritesAdd.
  ///
  /// In en, this message translates to:
  /// **'Added to favorites'**
  String get favoritesAdd;

  /// No description provided for @favoritesRemove.
  ///
  /// In en, this message translates to:
  /// **'Removed from favorites'**
  String get favoritesRemove;

  /// No description provided for @chatTitle.
  ///
  /// In en, this message translates to:
  /// **'Bookify Assistant'**
  String get chatTitle;

  /// No description provided for @chatHint.
  ///
  /// In en, this message translates to:
  /// **'Ask me anything...'**
  String get chatHint;

  /// No description provided for @chatGreeting.
  ///
  /// In en, this message translates to:
  /// **'Hello! I\'m Bookify\'s assistant. Ask me to find a business, check your booking, or answer a question.'**
  String get chatGreeting;

  /// No description provided for @chatSuggestFind.
  ///
  /// In en, this message translates to:
  /// **'Find a salon near me'**
  String get chatSuggestFind;

  /// No description provided for @chatSuggestBooking.
  ///
  /// In en, this message translates to:
  /// **'Check my booking status'**
  String get chatSuggestBooking;

  /// No description provided for @chatSuggestCancel.
  ///
  /// In en, this message translates to:
  /// **'How do I cancel a booking?'**
  String get chatSuggestCancel;

  /// No description provided for @chatSuggested.
  ///
  /// In en, this message translates to:
  /// **'Try asking:'**
  String get chatSuggested;

  /// No description provided for @chatError.
  ///
  /// In en, this message translates to:
  /// **'Could not send your message. Please try again.'**
  String get chatError;

  /// No description provided for @supportHelpCenter.
  ///
  /// In en, this message translates to:
  /// **'Help Center'**
  String get supportHelpCenter;

  /// No description provided for @supportContact.
  ///
  /// In en, this message translates to:
  /// **'Contact Support'**
  String get supportContact;

  /// No description provided for @supportReportProblem.
  ///
  /// In en, this message translates to:
  /// **'Report a Problem'**
  String get supportReportProblem;

  /// No description provided for @supportSubject.
  ///
  /// In en, this message translates to:
  /// **'Subject'**
  String get supportSubject;

  /// No description provided for @supportMessage.
  ///
  /// In en, this message translates to:
  /// **'Message'**
  String get supportMessage;

  /// No description provided for @supportCategory.
  ///
  /// In en, this message translates to:
  /// **'Category'**
  String get supportCategory;

  /// No description provided for @supportCategoryGeneral.
  ///
  /// In en, this message translates to:
  /// **'General'**
  String get supportCategoryGeneral;

  /// No description provided for @supportCategoryBooking.
  ///
  /// In en, this message translates to:
  /// **'Booking issue'**
  String get supportCategoryBooking;

  /// No description provided for @supportCategoryPayment.
  ///
  /// In en, this message translates to:
  /// **'Payment'**
  String get supportCategoryPayment;

  /// No description provided for @supportCategoryCancellation.
  ///
  /// In en, this message translates to:
  /// **'Cancellation'**
  String get supportCategoryCancellation;

  /// No description provided for @supportCategoryAccount.
  ///
  /// In en, this message translates to:
  /// **'Account'**
  String get supportCategoryAccount;

  /// No description provided for @supportCategoryProvider.
  ///
  /// In en, this message translates to:
  /// **'Provider question'**
  String get supportCategoryProvider;

  /// No description provided for @supportSubjectHint.
  ///
  /// In en, this message translates to:
  /// **'Brief summary of the issue'**
  String get supportSubjectHint;

  /// No description provided for @supportMessageHint.
  ///
  /// In en, this message translates to:
  /// **'Tell us what happened...'**
  String get supportMessageHint;

  /// No description provided for @supportContactEmail.
  ///
  /// In en, this message translates to:
  /// **'Contact email (optional)'**
  String get supportContactEmail;

  /// No description provided for @supportSubmitted.
  ///
  /// In en, this message translates to:
  /// **'Ticket submitted. We\'ll get back to you soon.'**
  String get supportSubmitted;

  /// No description provided for @supportFailed.
  ///
  /// In en, this message translates to:
  /// **'Could not submit. Please try again.'**
  String get supportFailed;

  /// No description provided for @supportReportAppointment.
  ///
  /// In en, this message translates to:
  /// **'Report an issue with this appointment'**
  String get supportReportAppointment;

  /// No description provided for @supportReportHint.
  ///
  /// In en, this message translates to:
  /// **'Describe what went wrong with this appointment'**
  String get supportReportHint;

  /// No description provided for @myBusinessTitle.
  ///
  /// In en, this message translates to:
  /// **'My Business'**
  String get myBusinessTitle;

  /// No description provided for @myBusinessListNew.
  ///
  /// In en, this message translates to:
  /// **'List a new business'**
  String get myBusinessListNew;

  /// No description provided for @myBusinessEmpty.
  ///
  /// In en, this message translates to:
  /// **'You have no businesses yet'**
  String get myBusinessEmpty;

  /// No description provided for @myBusinessEmptySubtitle.
  ///
  /// In en, this message translates to:
  /// **'List your business and start receiving bookings in minutes.'**
  String get myBusinessEmptySubtitle;

  /// No description provided for @myBusinessListYour.
  ///
  /// In en, this message translates to:
  /// **'List Your Business'**
  String get myBusinessListYour;

  /// No description provided for @myBusinessServices.
  ///
  /// In en, this message translates to:
  /// **'{count} services'**
  String myBusinessServices(Object count);

  /// No description provided for @myBusinessStaff.
  ///
  /// In en, this message translates to:
  /// **'{count} staff'**
  String myBusinessStaff(Object count);

  /// No description provided for @myBusinessResubmit.
  ///
  /// In en, this message translates to:
  /// **'Resubmit for Review'**
  String get myBusinessResubmit;

  /// No description provided for @myBusinessViewListing.
  ///
  /// In en, this message translates to:
  /// **'View Listing'**
  String get myBusinessViewListing;

  /// No description provided for @myBusinessAwaitingReview.
  ///
  /// In en, this message translates to:
  /// **'Awaiting review'**
  String get myBusinessAwaitingReview;

  /// No description provided for @myBusinessLive.
  ///
  /// In en, this message translates to:
  /// **'Your business is live and visible to customers.'**
  String get myBusinessLive;

  /// No description provided for @myBusinessPending.
  ///
  /// In en, this message translates to:
  /// **'Complete the checklist below and your business goes live automatically.'**
  String get myBusinessPending;

  /// No description provided for @myBusinessRejected.
  ///
  /// In en, this message translates to:
  /// **'Your listing was rejected by our review team.'**
  String get myBusinessRejected;

  /// No description provided for @myBusinessChecklist.
  ///
  /// In en, this message translates to:
  /// **'To go live, complete these steps:'**
  String get myBusinessChecklist;

  /// No description provided for @myBusinessPreview.
  ///
  /// In en, this message translates to:
  /// **'Preview how customers see it'**
  String get myBusinessPreview;

  /// No description provided for @myBusinessStatusApproved.
  ///
  /// In en, this message translates to:
  /// **'Live'**
  String get myBusinessStatusApproved;

  /// No description provided for @myBusinessStatusPending.
  ///
  /// In en, this message translates to:
  /// **'In progress'**
  String get myBusinessStatusPending;

  /// No description provided for @myBusinessStatusRejected.
  ///
  /// In en, this message translates to:
  /// **'Rejected'**
  String get myBusinessStatusRejected;

  /// No description provided for @myBusinessAutoNotice.
  ///
  /// In en, this message translates to:
  /// **'No admin approval needed — once the checklist is complete your business goes live automatically.'**
  String get myBusinessAutoNotice;

  /// No description provided for @onboardingTitle.
  ///
  /// In en, this message translates to:
  /// **'Welcome to Bookify'**
  String get onboardingTitle;

  /// No description provided for @onboardingSubtitle.
  ///
  /// In en, this message translates to:
  /// **'Create your business listing and start receiving bookings.'**
  String get onboardingSubtitle;
}

class _AppLocalizationsDelegate
    extends LocalizationsDelegate<AppLocalizations> {
  const _AppLocalizationsDelegate();

  @override
  Future<AppLocalizations> load(Locale locale) {
    return SynchronousFuture<AppLocalizations>(lookupAppLocalizations(locale));
  }

  @override
  bool isSupported(Locale locale) =>
      <String>['en', 'ur'].contains(locale.languageCode);

  @override
  bool shouldReload(_AppLocalizationsDelegate old) => false;
}

AppLocalizations lookupAppLocalizations(Locale locale) {
  // Lookup logic when only language code is specified.
  switch (locale.languageCode) {
    case 'en':
      return AppLocalizationsEn();
    case 'ur':
      return AppLocalizationsUr();
  }

  throw FlutterError(
    'AppLocalizations.delegate failed to load unsupported locale "$locale". This is likely '
    'an issue with the localizations generation tool. Please file an issue '
    'on GitHub with a reproducible sample app and the gen-l10n configuration '
    'that was used.',
  );
}
