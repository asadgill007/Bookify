import 'dart:math' as math;
import 'package:latlong2/latlong.dart';
import 'package:geolocator/geolocator.dart';

class MapsService {
  static const double _defaultLatitude = 40.7128;
  static const double _defaultLongitude = -74.0060;
  static const double _defaultZoom = 13.0;

  /// Get current user location
  Future<LatLng?> getCurrentLocation() async {
    try {
      bool serviceEnabled = await Geolocator.isLocationServiceEnabled();
      if (!serviceEnabled) {
        return null;
      }

      LocationPermission permission = await Geolocator.checkPermission();
      if (permission == LocationPermission.denied) {
        permission = await Geolocator.requestPermission();
        if (permission == LocationPermission.denied) {
          return null;
        }
      }

      if (permission == LocationPermission.deniedForever) {
        return null;
      }

      Position position = await Geolocator.getCurrentPosition(
        locationSettings: const LocationSettings(
          accuracy: LocationAccuracy.high,
        ),
      );

      return LatLng(position.latitude, position.longitude);
    } catch (e) {
      return null;
    }
  }

  /// Calculate distance between two points in kilometers
  double calculateDistance(LatLng from, LatLng to) {
    const earthRadius = 6371; // Earth's radius in kilometers
    
    double dLat = _toRadians(to.latitude - from.latitude);
    double dLon = _toRadians(to.longitude - from.longitude);
    
    double a =
        math.sin(dLat / 2) * math.sin(dLat / 2) +
        math.cos(_toRadians(from.latitude)) *
        math.cos(_toRadians(to.latitude)) *
        math.sin(dLon / 2) * math.sin(dLon / 2);
    
    double c = 2 * math.atan2(math.sqrt(a), math.sqrt(1 - a));
    
    return earthRadius * c;
  }

  /// Get default map center (New York City)
  LatLng getDefaultCenter() {
    return const LatLng(_defaultLatitude, _defaultLongitude);
  }

  /// Get default zoom level
  double getDefaultZoom() {
    return _defaultZoom;
  }

  /// Convert degrees to radians
  double _toRadians(double degrees) {
    return degrees * (3.14159265359 / 180);
  }

  /// Format distance for display
  String formatDistance(double distanceInKm) {
    if (distanceInKm < 1) {
      return '${(distanceInKm * 1000).toInt()} m';
    } else if (distanceInKm < 10) {
      return '${distanceInKm.toStringAsFixed(1)} km';
    } else {
      return '${distanceInKm.toInt()} km';
    }
  }
}