plugins {
    id("com.android.application")
    // The Flutter Gradle Plugin must be applied after the Android and Kotlin Gradle plugins.
    id("dev.flutter.flutter-gradle-plugin")
}

android {
    namespace = "com.bookify.bookify"
    compileSdk = flutter.compileSdkVersion
    ndkVersion = flutter.ndkVersion

    compileOptions {
        sourceCompatibility = JavaVersion.VERSION_17
        targetCompatibility = JavaVersion.VERSION_17
    }

    defaultConfig {
        // TODO: Specify your own unique Application ID (https://developer.android.com/studio/build/application-id.html).
        applicationId = "com.bookify.bookify"
        // You can update the following values to match your application needs.
        // For more information, see: https://flutter.dev/to/review-gradle-config.
        minSdk = flutter.minSdkVersion
        targetSdk = flutter.targetSdkVersion
        versionCode = flutter.versionCode
        versionName = flutter.versionName
    }

    buildTypes {
        release {
            // ═══════════════════════════════════════════════════════════════
            // IMPORTANT: Configure release signing before Play Store upload.
            // ═══════════════════════════════════════════════════════════════
            // 1. Create a keystore (do NOT commit it):
            //    keytool -genkey -v -keystore upload-keystore.jks \
            //      -storetype JKS -keyalg RSA -keysize 2048 -validity 9125 \
            //      -alias upload
            //
            // 2. Create android/key.properties with:
            //    storeFile=../upload-keystore.jks
            //    storePassword=<password>
            //    keyPassword=<password>
            //    keyAlias=upload
            //
            // 3. Add key.properties loading to build.gradle.kts:
            //    val keystorePropertiesFile = rootProject.file("key.properties")
            //    val keystoreProperties = java.util.Properties()
            //    if (keystorePropertiesFile.exists()) {
            //        keystoreProperties.load(keystorePropertiesFile.inputStream())
            //    }
            //
            // 4. Configure signingConfig:
            //    signingConfig = signingConfigs.create("release") {
            //        keyAlias = keystoreProperties["keyAlias"] as String
            //        keyPassword = keystoreProperties["keyPassword"] as String
            //        storeFile = keystoreProperties["storeFile"]?.let { file(it) }
            //        storePassword = keystoreProperties["storePassword"] as String
            //    }
            //
            // For now, using debug keys for development builds only.
            signingConfig = signingConfigs.getByName("debug")
        }
    }
}

kotlin {
    compilerOptions {
        jvmTarget = org.jetbrains.kotlin.gradle.dsl.JvmTarget.JVM_17
    }
}

flutter {
    source = "../.."
}
