# Import to discourse

This is used for importing content from [MVCForum](http://www.mvcforum.com/) to [discourse](http://www.discourse.org).
It builds the SQL code needed to add the content and adds it to the SQL dump script obtained from a discourse backup.

## What can be imported?
+ User accounts
+ Categories (the category description is mapped to category definition topic in discourse)
+ Topics
+ Tags on the topics
+ Posts

## Permalinks
With the software change, URLs for your content will also change.
For example, a thread found at `thread/forum-software-upgrade/` on MVCForum will have a URL like `t/forum-software-upgrade/118` on discourse.
The import code generates redirects from the URLs as used by MVCForum using the permalink feature in discourse.
The redirects enable your users to continue using their existing bookmarks and search engines to pick up the new URLs.

MVCForum models the post id in the 'fragment' portion of the URL which browsers normally don't send to the server.
For this reason, we cannot redirect from old post URL to new post URL.
To redirect post URLs to the containing topic, add the following regex transform in the 'permalink normalizations' setting in discourse: `/(thread.*)\?.*/\1`   

## Overall process of importing
+ Use the Backup feature in discourse to create an sql dump of the discourse database.
+ Use this program to add the data to be imported to the sql dump file.
+ Use the Restore feature in discourse with the modified sql dump.

## import from mvcforum
Import from [mvcforum](http://www.mvcforum.com/) supports users, categories, topics and posts.
The mvcforum database tables are exported into a single file using the sql script in the file `MvcForum.Export.To.Xml.sql`.

When running the import tool, supply the following arguments:
+ Path to the file containing the sql dump from discourse.
+ Path to the xml file containing the mvcforum export.

The tool then writes an sql script with the data merged from both databases into a new file.
After gzipping, you can apply it to your discourse instance using the restore function.