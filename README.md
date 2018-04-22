# aspcoreclass

## ICal-TimeZone-Override-Service
    
    X-WR-CALNAME:Training
    BEGIN:VEVENT
    DTEND:20180426T171500
    DTSTAMP:20180419T122646Z
    DTSTART:20180426T163000
    SEQUENCE:0

The above format is very common and results in the time being relative to Z also on the DTEND and DTSTART because no local timezone is specified nor can be derived from the ical file. 
Time formats are described in this post: https://stackoverflow.com/questions/10518804/formatting-time-for-ical-export

### Find your missing timezone
http://unicode.org/cldr/charts/32/supplemental/zone_tzid.html

### Live Service
http://icaltimezoneoverrideservice.azurewebsites.net/
