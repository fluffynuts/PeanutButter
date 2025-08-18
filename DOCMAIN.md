# About

PeanutButter is a collection of projects which provide
useful bits of functionality that I've often had to re-use.

# Quick links

This is by no means a comprehensive list of functionality that is available - it's just
some of the things I use the most, that may be helpful to others.

### Utilities
- Extension methods
  - [collections](classPeanutButter_1_1Utils_1_1ExtensionsForIEnumerables.html)
    - FindOrAdd for dictionaries
    - And - fluent arrays, eg foo.And(new [ 1, 2, 3 ]);
    - AsTextList and derivatives for dumping data neatly to the console or a textarea
    - Trim() on a collection of strings
    - JS-like methods (Map, Filter - working with array types)
  - [duck-typing](classPeanutButter_1_1DuckTyping_1_1Extensions_1_1DuckTypingObjectExtensions.html)
    - wrap any object in a well-defined interface and have your data & methods forwarded
    - works well on dictionaries too
    - useful for converting your app configuration to a well-defined object to pass around
    - can be used to "shield" a more complex type from a consumer only interested in
      a subset of the functionality
  - [objects](classPeanutButter_1_1Utils_1_1ObjectExtensions.html)
    - deep equality testing
    - reflection conveniences (eg o.Get<T>("PropertyName") / o.Set("PropertyName", "value")
    - o.DeepClone<T>()
  - [.Stringify()](classPeanutButter_1_1Utils_1_1Stringifier.html) to dump an object in "json-like" notation (good for logs / diagnostics)
  - [SlidingWindow](classPeanutButter_1_1Utils_1_1SlidingWindow-1-g.html): keep a running collection based on item count or max-age
  - [SingleItemCache](classPeanutButter_1_1Utils_1_1SingleItemCache-1-g.html): simple, easy way to implement localised caching for a value,
    eg an application setting which is queried a lot from a database
  - Disposables to make life easier:
    - [AutoTempFile](classPeanutButter_1_1Utils_1_1AutoTempFile.html) - deletes the temp file when disposed
    - [AutoTempFolder](classPeanutButter_1_1Utils_1_1AutoTempFolder.html) - deletes the temp folder and contents when disposed
    - [AutoLocker](classPeanutButter_1_1Utils_1_1AutoLocker.html) - locks the semaphore / mutex for the duration of the non-disposed lifetime
    - [AutoResetter](classPeanutButter_1_1Utils_1_1AutoResetter-1-g.html) - perform an action at instantiation, and undo it at disposal
    - [AutoTempEnvironmentVariable](classPeanutButter_1_1Utils_1_1AutoTempEnvironmentVariable.html) - set an environment variable for the duration of the non-disposed lifetime
    - [AutoBarrier](lassPeanutButter_1_1Utils_1_1AutoBarrier.html) - only exit the disposal block when all participants have joined
  - [CircularList](classPeanutButter_1_1Utils_1_1CircularList-1-g.html) - enumerate forever, cyclically, through a list
  - Dictionaries!
    - [DefaultDictionary](classPeanutButter_1_1Utils_1_1Dictionaries_1_1DefaultDictionary-2-g.html) (provide fallback value for missing keys)
    - Wrapper dictionaries for [ConnectionStringSettingCollections](classPeanutButter_1_1Utils_1_1Dictionaries_1_1DictionaryWrappingConnectionStringSettingCollection.html), [NameValueCollections](classPeanutButter_1_1Utils_1_1Dictionaries_1_1DictionaryWrappingNameValueCollection.html)
    - [MergeDictionary](classPeanutButter_1_1Utils_1_1Dictionaries_1_1MergeDictionary-2-g.html) - left-wise merge of underlying dictionaries with rewrite support
    - [RedirectingDictionary](classPeanutButter_1_1Utils_1_1Dictionaries_1_1RedirectingDictionary-1-g.html) - provide name redirection over an existing dictionary
    - [TransformingDictionary](classPeanutButter_1_1Utils_1_1Dictionaries_1_1TransformingDictionary-2-g.html) - passes values through a transformation layer on access
    - [ValidatingDictionary](classPeanutButter_1_1Utils_1_1Dictionaries_1_1ValidatingDictionary-2-g.html) - provides a dictionary which can be configured to validate keys and values
- Test utilities
  - [RandomValueGen](classPeanutButter_1_1RandomGenerators_1_1RandomValueGen.html) (suggest: import statically for less noise)
    - GetRandom(Int|Bool|String|Array<T>|Decimal|Date|Float|<T>)
      - GetRandom<T> can be guided via local implementations of GenericBuilder<TBuilder, TEntity>
  - [SimpleHTTPServer](namespacePeanutButter_1_1SimpleHTTPServer.html)
    - spin up a local HTTP server with low-level control 
      - useful for testing code that needs to have http comms validated or stubbed
  - Temporary database resources
    - [TempRedis](namespacePeanutButter_1_1TempRedis.html)
    - Temporary database instances
      - [TempDBMySqlData](namespacePeanutButter_1_1TempDb_1_1MySql_1_1Data.html) 
      - [TempDBMySqlConnector](namespacePeanutButter_1_1TempDb_1_1MySql_1_1Connector.html)
      - [TempDBLocalDb](namespacePeanutButter_1_1TempDb_1_1LocalDb.html) 
      - [TempDBSqlite](namespacePeanutButter_1_1TempDb_1_1Sqlite.html)
  - [Builders for faked asp.net types (http request/response, actions, etc) ](namespacePeanutButter_1_1TestUtils_1_1AspNetCore.html)
- [EasyArgs](namespacePeanutButter_1_1EasyArgs.html)
  - parses commandlines to an interface, can be guided with attributes on the interface
- [ServiceShell](namespacePeanutButter_1_1ServiceShell.html)
  - low-friction scaffolding for polling win32 services
- [INI reader/writer with comment preservation](namespacePeanutButter_1_1INI.html)
