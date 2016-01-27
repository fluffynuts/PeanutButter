      To use:
      1) Create a console application for your service
      2) derive from Shell
      3) provide a name for your service in the constructor of your derived class (this.ServiceName)
      4) set the polling interval if 10 seconds doesn't suit you
      5) Implement the RunOnce method in your derived class with your service logic
      6) your program main entry point just needs one line:
         Shell.RunMain&lt;YourClass>&gt;(args);
         where args is the complete string array passed into your main
     Note that your RunOnce needs to be synchronous -- so you can use tasks, just wait on them before leaving the
     method. The service shell will ensure that no more than your polling period elapses between polls; in other words,
	 if you set an interval of 10 seconds but a round of processing takes 15 seconds, your RunOnce will be immediately
	 invoked again after the last round, but no two rounds will run in parallel.
