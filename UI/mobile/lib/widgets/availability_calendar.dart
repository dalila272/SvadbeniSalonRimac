import 'package:flutter/material.dart';
import 'package:table_calendar/table_calendar.dart';
import 'package:svadbeni_salon_rimac/constants/colors.dart';

typedef DayAvailability = bool Function(DateTime day);

/// Monthly calendar where free days are green and unavailable days are muted.
class AvailabilityCalendar extends StatelessWidget {
  final DateTime focusedDay;
  final DateTime? selectedDay;
  final DateTime firstDay;
  final DateTime lastDay;
  final DayAvailability isDayAvailable;
  final ValueChanged<DateTime> onDaySelected;
  final ValueChanged<DateTime>? onPageChanged;
  final String freeLabel;
  final String busyLabel;

  const AvailabilityCalendar({
    super.key,
    required this.focusedDay,
    required this.selectedDay,
    required this.firstDay,
    required this.lastDay,
    required this.isDayAvailable,
    required this.onDaySelected,
    this.onPageChanged,
    this.freeLabel = 'Slobodan termin',
    this.busyLabel = 'Zauzet termin',
  });

  static final Color freeColor = Colors.green.shade100;
  static final Color freeText = Colors.green.shade900;
  static final Color busyColor = Colors.grey.shade300;
  static final Color busyText = Colors.grey.shade600;

  bool _sameDay(DateTime a, DateTime b) =>
      a.year == b.year && a.month == b.month && a.day == b.day;

  @override
  Widget build(BuildContext context) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        TableCalendar(
          firstDay: firstDay,
          lastDay: lastDay,
          focusedDay: focusedDay,
          startingDayOfWeek: StartingDayOfWeek.monday,
          calendarFormat: CalendarFormat.month,
          availableCalendarFormats: const {
            CalendarFormat.month: 'Mjesec',
          },
          headerStyle: const HeaderStyle(
            titleCentered: true,
            formatButtonVisible: false,
          ),
          selectedDayPredicate: (day) =>
              selectedDay != null && _sameDay(day, selectedDay!),
          enabledDayPredicate: (day) {
            if (day.isBefore(DateTime(firstDay.year, firstDay.month, firstDay.day))) {
              return false;
            }
            return isDayAvailable(day);
          },
          onDaySelected: (selected, focused) {
            if (!isDayAvailable(selected)) return;
            onDaySelected(DateTime(selected.year, selected.month, selected.day));
          },
          onPageChanged: onPageChanged,
          calendarBuilders: CalendarBuilders(
            defaultBuilder: (context, day, focused) =>
                _cell(day, available: isDayAvailable(day)),
            todayBuilder: (context, day, focused) => _cell(
              day,
              available: isDayAvailable(day),
              isToday: true,
            ),
            selectedBuilder: (context, day, focused) => _cell(
              day,
              available: true,
              isSelected: true,
            ),
            disabledBuilder: (context, day, focused) =>
                _cell(day, available: false),
            outsideBuilder: (context, day, focused) =>
                _cell(day, available: false, outside: true),
          ),
        ),
        const SizedBox(height: 8),
        Wrap(
          spacing: 16,
          runSpacing: 8,
          children: [
            _legendSwatch(freeColor, freeLabel),
            _legendSwatch(busyColor, busyLabel),
            _legendSwatch(
              AppColors.primaryColor.withValues(alpha: 0.25),
              'Odabrani dan',
            ),
          ],
        ),
      ],
    );
  }

  Widget _legendSwatch(Color color, String label) {
    return Row(
      mainAxisSize: MainAxisSize.min,
      children: [
        Container(
          width: 16,
          height: 16,
          decoration: BoxDecoration(
            color: color,
            borderRadius: BorderRadius.circular(4),
            border: Border.all(color: Colors.black26),
          ),
        ),
        const SizedBox(width: 6),
        Text(label, style: const TextStyle(fontSize: 12)),
      ],
    );
  }

  Widget _cell(
    DateTime day, {
    required bool available,
    bool isSelected = false,
    bool isToday = false,
    bool outside = false,
  }) {
    Color bg;
    Color fg;
    if (isSelected) {
      bg = AppColors.primaryColor;
      fg = Colors.white;
    } else if (available && !outside) {
      bg = freeColor;
      fg = freeText;
    } else {
      bg = busyColor;
      fg = busyText;
    }

    return Container(
      margin: const EdgeInsets.all(3),
      decoration: BoxDecoration(
        color: bg,
        borderRadius: BorderRadius.circular(8),
        border: isToday && !isSelected
            ? Border.all(color: AppColors.primaryColor, width: 1.5)
            : null,
      ),
      alignment: Alignment.center,
      child: Text(
        '${day.day}',
        style: TextStyle(
          color: fg,
          fontWeight: isSelected || available ? FontWeight.w600 : FontWeight.w400,
        ),
      ),
    );
  }
}
