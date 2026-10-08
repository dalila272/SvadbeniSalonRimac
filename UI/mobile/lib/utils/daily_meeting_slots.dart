class DailyMeetingSlots {
  static const duration = Duration(minutes: 30);

  /// Valid start times: 08:00–17:30 in 30-minute steps.
  static List<DateTime> slotsForDay(DateTime day) {
    final base = DateTime(day.year, day.month, day.day);
    final slots = <DateTime>[];
    for (var minutes = 8 * 60; minutes <= 17 * 60 + 30; minutes += 30) {
      slots.add(base.add(Duration(minutes: minutes)));
    }
    return slots;
  }

  static List<DateTime> freeSlotsForDay(
    DateTime day,
    List<DateTime> busySlots, {
    DateTime? excludeSlot,
    DateTime? after,
  }) {
    return slotsForDay(day).where((slot) {
      if (after != null && !slot.isAfter(after)) return false;
      return !isSlotTaken(slot, busySlots, excludeSlot: excludeSlot);
    }).toList();
  }

  static bool dayHasFreeSlot(
    DateTime day,
    List<DateTime> busySlots, {
    DateTime? excludeSlot,
    DateTime? after,
  }) {
    return freeSlotsForDay(
      day,
      busySlots,
      excludeSlot: excludeSlot,
      after: after,
    ).isNotEmpty;
  }

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
