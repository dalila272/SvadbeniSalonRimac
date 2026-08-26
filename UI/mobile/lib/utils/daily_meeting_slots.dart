class DailyMeetingSlots {
  static const duration = Duration(minutes: 30);

  static bool isValidStart(DateTime dateTime) {
    if (dateTime.minute != 0 && dateTime.minute != 30) return false;
    final minutes = dateTime.hour * 60 + dateTime.minute;
    return minutes >= 480 && minutes <= 1050;
  }

  static bool overlaps(DateTime startA, DateTime startB) {
    if (startA.year != startB.year ||
        startA.month != startB.month ||
        startA.day != startB.day) {
      return false;
    }
    final endA = startA.add(duration);
    final endB = startB.add(duration);
    return startA.isBefore(endB) && startB.isBefore(endA);
  }

  static bool isSlotTaken(
    DateTime slot,
    List<DateTime> busySlots, {
    DateTime? excludeSlot,
  }) {
    for (final busy in busySlots) {
      if (excludeSlot != null && busy == excludeSlot) continue;
      if (overlaps(busy, slot)) return true;
    }
    return false;
  }
}
