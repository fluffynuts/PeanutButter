PeanutButter Contributions Guide
---

First of all: welcome new contributor!

Contributions are appreciated, with the following kept in mind:

1. Contributions may not break behaviors for existing users unless agreed upon that doing so is in the best interests of the project involved and the PeanutButter suite affects a MAJOR version increment.
2. Contributions may not contain hate speech or anything considered defamatory towards others. We're all in this together: no need to be mean.
3. Contributions _must_ be accompanied by corresponding test code to be accepted. It is acceptable to raise a "work-in-progess" pull request and ask for help with testing if you're struggling, but untested code will not get into PeanutButter. Unit tests should all run and pass, and should use the NUnit test framework for fixtures. Preferably, assertions should be done with `NExpect` - see examples in the existing code-base. New tests should run alongside existing ones and should be runnable from the commandline. Your tests should be triggered via `npm test` at the command-line.
4. Contributions _should_ be in the interest of the majority of users. In the case where minor behavioral changes might be required, but said changes are probably not what most people want, the discussion should rather be around how to allow consuming code to _inject_ those behaviors.
5. All rights are reserved to reject contributions. It is expected that a rejection should be accompanied by a clear motivation.
6. All rights are reserved to re-implement a pending contribution if that contribution is spending too long in code review, appears to be abandoned, or the author thereof refuses to make changes from code review. Basically this means that if a PR is abandoned or doesn't meet the quality standards required, then the implementation logic may be included into PeanutButter without accepting the PR. It is preferable, however, for pull requests to come to an amiable conclusion and be accepted, such that the original author appears in the commit log.
7. Contributions which contain code from other sources must cite those sources. Contributions may not contravene any legal or ethical boundaries, where discernment of the latter is up to the discretion of the repository maintainer(s).
8. Code style should be preserved. Code review will be held up until contribution code fits in with existing styles.