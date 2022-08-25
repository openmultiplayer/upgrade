 Notes
-------

* Most defines aren't needed.

There are very strict specific defines for various number types (float/hex/boolean), symbols, and more.  The truth is that for most use-cases this specificity is not needed and isn't used.  When scanning for an expression the system only needs to know that two brackets are matched for example, it doesn't need to know that the stuff within the brackets is a fully conforming hex number for example.  Even the strict symbol definition is barely used, so this will match names that start with numbers, even though it shouldn't.

* Replacements are ordered.

So for example `Upgrading hooks to decorators` comes before `Adding 'CLICK_SOURCE' tag to 'OnPlayerClickPlayer'`, and the latter only looks for `public|forward|@hook` not `public|forward|hook|HOOK__|@hook`, because it will know that the other two can't exist any more.  Hence why they use an array.

Defines are not ordered, hence why they use an object.

* Why does this use PCRE.NET instead of the inbuilt regex grammar?

Simple - while the inbuilt grammar does have *expression balancing* which can be used to match `()`s in expressions it doesn't have *subroutines*, so while the complex expressions **can** be built up, they can't be abstracted.  Using PCRE allows things like the regex for a function parameter to be placed in a separate `DEFINE` and reused in all the scanners by name.  The .NET version would require those expressions to be copied and pasted in to every scanner; which would entirely obfuscate their basic use and require massive updates for every single bug fix.

