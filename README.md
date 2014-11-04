Jinx
====

SQL Map Reduce Job Runner


Small things to do
=================
1.  Move ``BuildSrcModelDictionary`` in JobService to Chronos under a new section called something like SqlQueryMetadata or something like that
2.  Same thing with ``BuildModel``, give it more descriptive name.  (all it does is take a dictionary of ``string``, and ``Type`` and generates a class (as a string) out of it, and in ability to pass a list of namespaces, and the actual model name
3.  As part of the Metadata stuff in chronos, add a method that can take in a connection string, and db type, and give back a list of tables.  Once we have that, change the textboxes that ask to insert a table name with dropdowns
4.  Moved the editJobCtrl angular controller out of the EditJob view, see if we can combine any logic with the createJobCtrl (they both do nearly the same thing)
