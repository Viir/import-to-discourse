# Import to discourse

This is used for importing content from [MVCForum](http://www.mvcforum.com/) to [discourse](http://www.discourse.org).
It builds the SQL code needed to add the content and adds it to the SQL dump script obtained from a discourse backup.

## What can be imported?
+ User accounts
+ Categories (the category description is mapped to category definition topic in discourse)
+ Topics
+ Tags on the topics
+ Posts
+ Upvotes on posts (mapped to likes in discourse)

## Permalinks
With the software change, URLs for your content will also change.
For example, a thread found at `thread/forum-software-upgrade/` on MVCForum will have a URL like `t/forum-software-upgrade/118` on discourse.
The import code generates redirects from the URLs as used by MVCForum using the permalinks table in discourse.
The redirects enable your users to continue using their existing bookmarks and search engines to pick up the new URLs.

MVCForum models the post id in the 'fragment' portion of the URL which browsers normally don't send to the server.
For this reason, we cannot redirect from old post URL to new post URL.
To redirect post URLs to the containing topic, add the following regex transform in the 'permalink normalizations' setting in discourse: `/(thread.*)\?.*/\1`   

## Overall process of importing
+ Use the Backup feature in discourse to create an SQL dump of the discourse database.
+ Use this program to add the data to be imported to the SQL dump file.
+ Use the Restore feature in discourse with the modified SQL dump.

## Merging the MVCForum data into the SQL dump
First, export the data from the MVCForum database into a single file using the sql script in the file [`MvcForum.Export.To.Xml.sql`](https://github.com/Viir/import-to-discourse/blob/master/import-to-discourse/MvcForum.Export.To.Xml.sql).

Then run the import tool and supply the following arguments:
+ Path to the file containing the sql dump from discourse.
+ Path to the xml file containing the MVCForum export.

The tool then writes an sql script with the data merged from both databases into a new file.
After gzipping, you can apply it to your discourse instance using the restore function.


```
K:\BotEngine.Forum.Upgrade>tool\import_to_discourse.exe botengine-forum-2016-12-10-142315-v20161205065743.sql BotEngine.Forum.mvcforum.Export.xml
Tool from https://github.com/Viir/import-to-discourse to import from mvcforum to discourse.
Warning: diagnostics are not implemented. For example, importing a user with an email address that was already present in the database was observed to result in a reset of discourse.
Press key to continue.
Reading discourse database dump from file: "botengine-forum-2016-12-10-142315-v20161205065743.sql"
Reading data to be imported from file: "BotEngine.Forum.mvcforum.Export.xml"
Imported 162 user accounts, 9 categories, 93 topics and 698 posts in 3887 ms.
Starting to write sql script to file botengine-forum-2016-12-10-142315-v20161205065743.sql.with.imported.2016-12-10T142808.sql
Completed in 4004 ms.

```
