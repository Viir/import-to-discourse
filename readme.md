# Import to discourse

This is used for importing content to a [discourse](http://www.discourse.org) instance.
It builds the sql code needed to add the content and adds it to the sql dump script obtained from a discourse backup.

### Overall process of importing
+ Create a backup in discourse.
+ Use this program to add the content to the sql dump in the file `dump.sql` contained in the backup.
+ Restore from the modified backup.

### Repackaging the backup with modified sql dump
With 7-Zip, files can easily be replaced in the nested structure of the backup file as it takes care of propagating the changes upwards.
If a file has been replaced in the 7-Zip GUI, it asks if the containing archive(s) should be updated as well upon closing the GUI.
This seems to work at least for the gz->tar->gz hierarchy as seen in discourse backups.

