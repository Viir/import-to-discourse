# Import to discourse

This is used for importing content to a [discourse](http://www.discourse.org) instance.
It builds the sql code needed to add the content and adds it to the sql dump script obtained from a discourse backup.

### Overall process of importing
+ Use the Backup feature in discourse to create an sql dump of the discourse database.
+ Use this program to add the data to be imported to the sql dump file.
+ Use the Restore feature in discourse with the modified sql dump.

## import from mvcforum
Import from [mvcforum](http://www.mvcforum.com/) supports users, categories, topics and posts.
The mvcforum database tables are exported into a single file using the sql script in the file `MvcForum.Export.To.Xml.sql`.
