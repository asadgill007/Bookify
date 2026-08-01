// ignore: unused_import
import 'package:intl/intl.dart' as intl;
import 'app_localizations.dart';

// ignore_for_file: type=lint

/// The translations for Urdu (`ur`).
class AppLocalizationsUr extends AppLocalizations {
  AppLocalizationsUr([String locale = 'ur']) : super(locale);

  @override
  String get appTitle => 'بکائفائی';

  @override
  String get appTagline => 'اپنی پسندیدہ سروس تلاش کریں';

  @override
  String get appSubtitle =>
      'AI سے چلنے والا عالمی ملٹی سروس اپائنٹمنٹ بکنگ پلیٹ فارم';

  @override
  String get navHome => 'ہوم';

  @override
  String get navSearch => 'تلاش';

  @override
  String get navAppointments => 'اپائنٹمنٹس';

  @override
  String get navProfile => 'پروفائل';

  @override
  String get commonCancel => 'منسوخ';

  @override
  String get commonSave => 'محفوظ کریں';

  @override
  String get commonDelete => 'حذف کریں';

  @override
  String get commonRetry => 'دوبارہ کوشش';

  @override
  String get commonClose => 'بند کریں';

  @override
  String get commonSubmit => 'جمع کریں';

  @override
  String get commonSend => 'بھیجیں';

  @override
  String get commonSearch => 'تلاش';

  @override
  String get commonLoading => 'لوڈ ہو رہا ہے...';

  @override
  String get commonError => 'کچھ غلط ہو گیا';

  @override
  String get commonConfirm => 'تصدیق کریں';

  @override
  String get commonBack => 'واپس';

  @override
  String get commonYes => 'ہاں';

  @override
  String get commonNo => 'نہیں';

  @override
  String get commonNext => 'اگلا';

  @override
  String get commonOptional => '(اختیاری)';

  @override
  String get commonNoResults => 'کوئی نتیجہ نہیں ملا';

  @override
  String get commonViewAll => 'سب دیکھیں';

  @override
  String get commonSettings => 'ترتیبات';

  @override
  String get commonNotifications => 'اطلاعات';

  @override
  String get commonSupport => 'سپورٹ';

  @override
  String get commonLanguage => 'زبان';

  @override
  String get commonCurrency => 'کرنسی';

  @override
  String get commonAppearance => 'ظاہری شکل';

  @override
  String get commonAccount => 'اکاؤنٹ';

  @override
  String get commonEditProfile => 'پروفائل میں ترمیم';

  @override
  String get commonLogout => 'سائن آؤٹ';

  @override
  String get commonVerified => 'تصدیق شدہ';

  @override
  String get homeDiscover => 'دریافت کریں';

  @override
  String get homeSearchHint => 'خدمات تلاش کریں...';

  @override
  String get homeCategories => 'زمرے';

  @override
  String get homeFeatured => 'نمایاں کاروبار';

  @override
  String get homeChatBot => 'بکائفائی اسسٹنٹ سے بات کریں';

  @override
  String get searchHint => 'کاروبار تلاش کریں...';

  @override
  String get searchEmpty => 'ڈاکٹر، سیلون، سپا تلاش کریں...';

  @override
  String get searchEmptySubtitle => 'نام، زمرے یا مقام سے تلاش کریں';

  @override
  String get searchFailed => 'تلاش ناکام';

  @override
  String searchNoResults(Object query) {
    return '\"$query\" کے لیے کوئی نتیجہ نہیں ملا';
  }

  @override
  String get searchNoResultsSubtitle => 'کوئی اور لفظ آزمائیں';

  @override
  String get searchFilterByCategory => 'زمرے کے مطابق فلٹر کریں';

  @override
  String get searchNoCategories => 'کوئی زمرہ دستیاب نہیں';

  @override
  String get searchFilters => 'فلٹرز';

  @override
  String get searchFilterPriceRange => 'قیمت کی حد';

  @override
  String get searchFilterDistance => 'فاصلے کا دائرہ';

  @override
  String get searchFilterRating => 'کم از کم ریٹنگ';

  @override
  String get searchFilterCategory => 'زمرہ';

  @override
  String get searchFilterClear => 'فلٹر صاف کریں';

  @override
  String get searchFilterApply => 'لاگو کریں';

  @override
  String get searchAnyPrice => 'کوئی بھی قیمت';

  @override
  String get searchAnyDistance => 'کہیں بھی';

  @override
  String get searchAnyRating => 'کوئی بھی ریٹنگ';

  @override
  String get searchActiveFilters => 'فعال فلٹرز';

  @override
  String searchKm(Object km) {
    return '$km کلومیٹر';
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
  String get profileTitle => 'پروفائل';

  @override
  String get profileBookings => 'بکنگز';

  @override
  String get profileEditProfile => 'پروفائل میں ترمیم';

  @override
  String get profileMyAppointments => 'میرے اپائنٹمنٹس';

  @override
  String get profileMyWaitlist => 'میری ویٹ لسٹ';

  @override
  String get profileRecurring => 'بار بار کی بکنگ';

  @override
  String get profileMyBusiness => 'میرا کاروبار';

  @override
  String get profileReviewBusinesses => 'کاروبار کا جائزہ لیں';

  @override
  String get profileFavorites => 'پسندیدہ';

  @override
  String get profileHelp => 'مدد کا مرکز';

  @override
  String get profileAbout => 'کے بارے میں';

  @override
  String get profileContactSupport => 'سپورٹ سے رابطہ کریں';

  @override
  String get profileReportProblem => 'مسئلہ رپورٹ کریں';

  @override
  String get profileTerms => 'خدمات کی شرائط';

  @override
  String get profilePrivacy => 'رازداری کی پالیسی';

  @override
  String get settingsTitle => 'ترتیبات';

  @override
  String get settingsDarkMode => 'ڈارک موڈ';

  @override
  String get settingsSystem => 'سسٹم';

  @override
  String get settingsLight => 'لائٹ';

  @override
  String get settingsDark => 'ڈارک';

  @override
  String get settingsLanguage => 'زبان';

  @override
  String get settingsCurrency => 'کرنسی';

  @override
  String get settingsNotifications => 'اطلاعات';

  @override
  String get settingsPushNotifications => 'پش اطلاعات';

  @override
  String get settingsPushSubtitle => 'بکنگ اپ ڈیٹس وصول کریں';

  @override
  String get settingsEmailNotifications => 'ای میل اطلاعات';

  @override
  String get settingsEmailSubtitle => 'پروموشنل ای میلز وصول کریں';

  @override
  String get settingsChangePassword => 'پاس ورڈ تبدیل کریں';

  @override
  String get settingsDeleteAccount => 'اکاؤنٹ حذف کریں';

  @override
  String get settingsDeleteAccountSubtitle =>
      'یہ آپ کا اکاؤنٹ اور تمام ڈیٹا مستقل طور پر حذف کر دے گا۔';

  @override
  String get settingsDeleting => 'اکاؤنٹ حذف ہو رہا ہے...';

  @override
  String get settingsSelectLanguage => 'زبان منتخب کریں';

  @override
  String get settingsSelectCurrency => 'کرنسی منتخب کریں';

  @override
  String get authWelcomeBack => 'خوش آمدید';

  @override
  String get authSignInSubtitle =>
      'اپنے اپائنٹمنٹس کے انتظام کے لیے سائن ان کریں';

  @override
  String get authEmail => 'ای میل';

  @override
  String get authPassword => 'پاس ورڈ';

  @override
  String get authSignIn => 'سائن ان';

  @override
  String get authForgotPassword => 'پاس ورڈ بھول گئے؟';

  @override
  String get authNoAccount => 'اکاؤنٹ نہیں ہے؟ ';

  @override
  String get authCreateAccount => 'اکاؤنٹ بنائیں';

  @override
  String get authSignInWithGoogle => 'گوگل سے سائن ان کریں';

  @override
  String get authEmailRequired => 'ای میل درکار ہے';

  @override
  String get authValidEmail => 'درست ای میل درج کریں';

  @override
  String get authPasswordRequired => 'پاس ورڈ درکار ہے';

  @override
  String get authPasswordMin => 'پاس ورڈ کم از کم 6 حروف کا ہو';

  @override
  String get authCreateSubtitle =>
      'بکائفائی میں شامل ہوں اور پریمیم خدمات بک کریں';

  @override
  String get authFirstName => 'پہلا نام';

  @override
  String get authLastName => 'آخری نام';

  @override
  String get authConfirmPassword => 'پاس ورڈ کی تصدیق';

  @override
  String get authRequired => 'لازمی';

  @override
  String get authAtLeast8 => 'کم از کم 8 حروف';

  @override
  String get authPasswordsMatch => 'پاس ورڈ مماثل نہیں';

  @override
  String get authAlreadyAccount => 'پہلے سے اکاؤنٹ ہے؟ ';

  @override
  String get authCustomer => 'کسٹمر';

  @override
  String get authCustomerSubtitle => 'اپنے قریب خدمات بک کریں';

  @override
  String get authListBusiness => 'اپنا کاروبار درج کریں';

  @override
  String get authListBusinessSubtitle =>
      'مالکان و عملہ بکنگز کے انتظام کے لیے شامل ہوں';

  @override
  String get authGoogleError =>
      'گوگل سائن ان اس بلڈ کے لیے ترتیب شدہ نہیں ہے۔ --dart-define=GOOGLE_CLIENT_ID شامل کریں۔';

  @override
  String get businessBookNow => 'ابھی بک کریں';

  @override
  String get businessAbout => 'تعارف';

  @override
  String get businessGallery => 'گیلری';

  @override
  String get businessServices => 'خدمات';

  @override
  String get businessOurTeam => 'ہماری ٹیم';

  @override
  String get businessOpeningHours => 'کھلنے کے اوقات';

  @override
  String get businessClosed => 'بند';

  @override
  String get businessNoStaff => 'عملہ نہیں';

  @override
  String get businessBook => 'بک کریں';

  @override
  String businessMinutes(Object minutes) {
    return '$minutes منٹ';
  }

  @override
  String businessReviews(Object count) {
    return '($count جائزے)';
  }

  @override
  String get favoritesTitle => 'میری پسندیدہ';

  @override
  String get favoritesEmpty => 'ابھی کوئی پسندیدہ نہیں';

  @override
  String get favoritesEmptySubtitle =>
      'کسی بھی کاروبار پر دل کا نشان دبائیں تاکہ یہ یہاں محفوظ ہو';

  @override
  String get favoritesAdd => 'پسندیدہ میں شامل';

  @override
  String get favoritesRemove => 'پسندیدہ سے ہٹا دیا گیا';

  @override
  String get chatTitle => 'بکائفائی اسسٹنٹ';

  @override
  String get chatHint => 'مجھ سے کچھ بھی پوچھیں...';

  @override
  String get chatGreeting =>
      'سلام! میں بکائفائی کا اسسٹنٹ ہوں۔ کاروبار تلاش کرنے، بکنگ چیک کرنے یا سوال پوچھنے کے لیے پوچھیں۔';

  @override
  String get chatSuggestFind => 'میرے قریب سیلون تلاش کریں';

  @override
  String get chatSuggestBooking => 'میری بکنگ کی حیثیت چیک کریں';

  @override
  String get chatSuggestCancel => 'بکنگ کیسے منسوخ کریں؟';

  @override
  String get chatSuggested => 'آزمائیں:';

  @override
  String get chatError => 'آپ کا پیغام نہیں بھیجا جا سکا۔ دوبارہ کوشش کریں۔';

  @override
  String get supportHelpCenter => 'مدد کا مرکز';

  @override
  String get supportContact => 'سپورٹ سے رابطہ کریں';

  @override
  String get supportReportProblem => 'مسئلہ رپورٹ کریں';

  @override
  String get supportSubject => 'موضوع';

  @override
  String get supportMessage => 'پیغام';

  @override
  String get supportCategory => 'زمرہ';

  @override
  String get supportCategoryGeneral => 'عام';

  @override
  String get supportCategoryBooking => 'بکنگ مسئلہ';

  @override
  String get supportCategoryPayment => 'ادائیگی';

  @override
  String get supportCategoryCancellation => 'منسوخی';

  @override
  String get supportCategoryAccount => 'اکاؤنٹ';

  @override
  String get supportCategoryProvider => 'پرووائیڈر سوال';

  @override
  String get supportSubjectHint => 'مسئلے کا مختصر خلاصہ';

  @override
  String get supportMessageHint => 'ہمیں بتائیں کیا ہوا...';

  @override
  String get supportContactEmail => 'رابطہ ای میل (اختیاری)';

  @override
  String get supportSubmitted => 'ٹکٹ جمع ہو گیا۔ ہم جلد رابطہ کریں گے۔';

  @override
  String get supportFailed => 'جمع نہیں ہو سکا۔ دوبارہ کوشش کریں۔';

  @override
  String get supportReportAppointment =>
      'اس اپائنٹمنٹ کے ساتھ مسئلہ رپورٹ کریں';

  @override
  String get supportReportHint =>
      'اس اپائنٹمنٹ کے ساتھ کیا مسئلہ ہوا بیان کریں';

  @override
  String get myBusinessTitle => 'میرا کاروبار';

  @override
  String get myBusinessListNew => 'نیا کاروبار درج کریں';

  @override
  String get myBusinessEmpty => 'آپ کے پاس ابھی کوئی کاروبار نہیں';

  @override
  String get myBusinessEmptySubtitle =>
      'اپنا کاروبار درج کریں اور منٹوں میں بکنگ وصول کریں۔';

  @override
  String get myBusinessListYour => 'اپنا کاروبار درج کریں';

  @override
  String myBusinessServices(Object count) {
    return '$count خدمات';
  }

  @override
  String myBusinessStaff(Object count) {
    return '$count عملہ';
  }

  @override
  String get myBusinessResubmit => 'جائزے کے لیے دوبارہ جمع کریں';

  @override
  String get myBusinessViewListing => 'لسٹنگ دیکھیں';

  @override
  String get myBusinessAwaitingReview => 'جائزے کا انتظار';

  @override
  String get myBusinessLive =>
      'آپ کا کاروبار لائیو ہے اور صارفین کو نظر آ رہا ہے۔';

  @override
  String get myBusinessPending =>
      'نیچے دی گئی چیک لسٹ مکمل کریں اور آپ کا کاروبار خود بخود لائیو ہو جائے گا۔';

  @override
  String get myBusinessRejected => 'آپ کی لسٹنگ جائزہ ٹیم نے مسترد کر دی۔';

  @override
  String get myBusinessChecklist => 'لائیو ہونے کے لیے یہ اقدامات مکمل کریں:';

  @override
  String get myBusinessPreview => 'دیکھیں صارفین اسے کیسے دیکھتے ہیں';

  @override
  String get myBusinessStatusApproved => 'لائیو';

  @override
  String get myBusinessStatusPending => 'جاری';

  @override
  String get myBusinessStatusRejected => 'مسترد';

  @override
  String get myBusinessAutoNotice =>
      'ایڈمن منظوری کی ضرورت نہیں — چیک لسٹ مکمل ہوتے ہی آپ کا کاروبار خود بخود لائیو ہو جاتا ہے۔';

  @override
  String get onboardingTitle => 'بکائفائی میں خوش آمدید';

  @override
  String get onboardingSubtitle =>
      'اپنا کاروبار درج کریں اور بکنگ وصول کرنا شروع کریں';
}
