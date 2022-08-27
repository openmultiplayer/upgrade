This program attempts to find old untagged and const-incorrect code and upgrade it.  There are many pre-defined upgrade replacements and you can create your own either manually or automatically.

 Usage
-------

To replace most native declarations and hooks with const-correct equivalents use:

```
upgrade --scans const ../qawno/include
```

This will load all the replacements from `./const.json` and apply them to every file recursively in `../qawno/include`.

If you don't want to apply the changes immediately use:

```
upgrade --report --scans const ../qawno/include
```

Which will instead just tell you of the changes to be made.

 Generation
------------

To create your own replacements file for a newly tagged function you need the definitions of the values, and the new prototype of the function using `$` in place of the tag in `_generate.json`:

```
{
	"enum": "enum Colour: { RED, GREEN, BLUE }",
	"code": "stock SetIndexedColour(playerid, $:colour)"
}
```

Then use the generate command:

```
upgrade --generate
```

This will print the upgrade `.json` for replacing all similar declarations (a *similar* declaration being one that looks like `Prefix_Name` where `Prefix_` is optional and `Name` is the function name specified) and all uses.  The uses will attempt to find existing calls that look like `SetIndexedColour(playerid, 2)` and replace them with their tagged constant equivalents: `SetIndexedColour(playerid, BLUE)`.

 Notes
-------

* Most defines aren't needed.

There are very strict specific defines for various number types (float/hex/boolean), symbols, and more.  The truth is that for most use-cases this specificity is not needed and isn't used.  When scanning for an expression the system only needs to know that two brackets are matched for example, it doesn't need to know that the stuff within the brackets is a fully conforming hex number for example.  Even the strict symbol definition is barely used, so this will match names that start with numbers, even though it shouldn't.

* Replacements are ordered.

So for example `Upgrading hooks to decorators` comes before `Adding 'CLICK_SOURCE' tag to 'OnPlayerClickPlayer'`, and the latter only looks for `public|forward|@hook` not `public|forward|hook|HOOK__|@hook`, because it will know that the other two can't exist any more.  Hence why they use an array.

Defines are not ordered, hence why they use an object.

* Why does this use PCRE.NET instead of the inbuilt regex grammar?

Simple - while the inbuilt grammar does have *expression balancing* which can be used to match `()`s in expressions it doesn't have *subroutines*, so while the complex expressions **can** be built up, they can't be abstracted.  Using PCRE allows things like the regex for a function parameter to be placed in a separate `DEFINE` and reused in all the scanners by name.  The .NET version would require those expressions to be copied and pasted in to every scanner; which would entirely obfuscate their basic use and require massive updates for every single bug fix.

* Use `--debug` to view the regex being run.

Especially useful for trying to figure out why a replacement you think should have been made wasn't.

