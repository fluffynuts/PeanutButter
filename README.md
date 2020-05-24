PeanutButter
============
![Test status](https://github.com/fluffynuts/PeanutButter/workflows/Tests/badge.svg)

"Some tastiness to add to your dev sandwich"

PeanutButter is a collection of projects which provide useful
bits of functionality that I've often had to re-use (and also
provides some concrete examples of usage). Sometimes I can even
be bothered to write about it at
[http://davydm.blogspot.co.za/search/label/PeanutButter](http://davydm.blogspot.co.za/search/label/PeanutButter).

Inside, you'll find, amongst other things:

* Duck-typing for .NET (PeanutButter.DuckTyping)
  - rigid or fuzzy duck-typing, including the ability
    to wrap a dictionary with a strongly-typed interface,
    even if the starting dictionary is empty.
* Randomisation utilities for testing with (PeanutButter.RandomGenerators)
  - Random Value Generators
    - Tests with random values are usually more useful because,
      if nothing else, after many runs, you've hit many test
      scenarios and often an edge case which would cause you
      production headaches pops out of the wood work. I prefer
      to test with random values wherever possible
    - In addition to the helpers in RandomValueGen which can
      be used to get randomised:
      - string values (arbitrary, alphabetic, or alphanumeric)
      - numeric values (decimal, long)
      - boolean values
      - datetime values
      there is also the GenericBuilder base which you can use
      as a very quick and easy way to create builders for
      complex types. These builders have the ability to generate
      randomised or directed objects.
* Test utilities:
  - PeanutButter.TestUtils.Generic which allows easy TestCase
    scenarios for property get/set tests with PropertyAssert
    which allows easy comparison of properties by name or, indeed
    relative path from the objects provided
  - PeanutButter.TestUtils.MVC which provides a JsonResultExtensions
    class to simplify testing MVC actions which return JsonResults
  - PeanutButter.MVC, which provides facades and interfaces to make
    script and style bundles testable
  - PeanutButter.TestUtils.Entity provides mechanisms for testing
    your EF-based project code against temporary databases
    so you can be sure that the code you deploy will work as expected.
    This library is supported by PeanutButter.TempDb, so you can test
    (out of the box) against LocalDb, Sqlite and SQLCE. You can also
    provide your own TempDb<> implmentation
* Arbitrary utils
  - DecimalDecorator which provides relatively safe string/decimal
    interchange, irrespective of culture
  - XElementExtensions to make dealing with XElement text easier
* On-the-fly HTTP server for testing when you simply need a WebRequest
    to work
* TempDb implementations (LocalDb, SqlCe and Sqlite) so you can run
    tests which involve migrations and integration between your ORM
    and the actual db
* WindowsServiceManagement
  - provides functionality for query of & interaction with win32
    services using the native Win32 api, exposed in an easy-to-use
    C# POCO
* PeanutButter.DatabaseHelpers
  - provides builders for Select, Update, Delete and Insert SQL
    statements for legacy projects where you can't (at least, not
    yet) get in an ORM. The builders are more testable and you can
    use compile-time constants for table/column names to help to
    harden your code against dev errors and code rot
  - provides OleDB database executors and a data reader builder
* PeanutButter.ServiceShell
  - provides scaffolding for a polling win32 service: inherit
    from the ServiceShell class, configure name and interval
    and provide your custom logic in the RunOnce override
  - resultant services can install and uninstall themselves as
    well as be invoked for once-off runs from the commandline.
    The project also contains a simple, but effective log4net
    configuration for console and a log file
* EmailSpooler.Win32Service
  - harnesses PeanutButter.ServiceShell to provide a generic
    SMTP email spooling service backed with a data store. You
    can (theoretically) use any database which Entity can connect
    to, though this project has only been tested against MSSQL
  - EmailSpooler.Win32Service.DB provides FluentMigrator and
    raw SQL script files for generating the required underlying
    data structures.

Barring the last item, none of these are stand-alone: they're all
just building blocks which I've had to repeat and refine, so I
figure they may be of use to others. As far as possible, the code
should be under test, though some projects are more difficult to
unit test (eg the PeanutButter.WindowsServiceManagement project,
which was developed TDD-style but which would sometimes flake out
on tests because the windows service management would be hit too hard
or often. But it does work (:) And some are libraries to help with
testing, so you'll soon find that they work as expected.

<center>A shout out to:

![Jetbrains Logo](logo_JetBrains_4.png)
</center>
The work on PeanutButter would have been a lot more effort without
ReSharper from JetBrains. The good people at JetBrains have provided
free licensing for all of their products for open-source projects like
this one. To learn more about JetBrains products, please [visit them](http://jetbrains.com)
