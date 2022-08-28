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

 Tag Example
-------------

Given the following file:

```pawn
native SetPlayerObjectMaterialText(playerid, objectid, text[], materialindex = 0, materialsize = OBJECT_MATERIAL_SIZE_256x128, fontface[] = "Arial", fontsize = 24, bold = 1, fontcolor = 0xFFFFFFFF, backcolor = 0, textalignment = 0);
SetTimer("MyFunction", 2000, 0);
```

And parameters:

```
--scans tags ../qawno/include
```

The report output will be:

```
Scanning file: D:\open.mp\upgrade\example.inc

    @@ -1,1 +1,1 @@ Add tags to `SetPlayerObjectMaterialText`
    -native SetPlayerObjectMaterialText(playerid, objectid, text[], materialindex = 0, materialsize = OBJECT_MATERIAL_SIZE_256x128,
    +native bool:SetPlayerObjectMaterialText(playerid, objectid, text[], materialindex = 0, OBJECT_MATERIAL_SIZE:materialsize = OBJECT_MATERIAL_SIZE_256x128,
    @@ -1,1 +1,1 @@ Add tags to `SetPlayerObjectMaterialText`
    -native bool:SetPlayerObjectMaterialText(playerid, objectid, text[], materialindex = 0, OBJECT_MATERIAL_SIZE:materialsize = OBJECT_MATERIAL_SIZE_256x128, fontface[] = "Arial", fontsize = 24, bold = 1, fontcolor = 0xFFFFFFFF, backcolor = 0, textalignment = 0)
    +native bool:SetPlayerObjectMaterialText(playerid, objectid, text[], materialindex = 0, OBJECT_MATERIAL_SIZE:materialsize = OBJECT_MATERIAL_SIZE_256x128, fontface[] = "Arial", fontsize = 24, bold = 1, fontcolor = 0xFFFFFFFF, backcolor = 0, OBJECT_MATERIAL_TEXT_ALIGN:textalignment = OBJECT_MATERIAL_TEXT_ALIGN_LEFT)
    @@ -2,1 +2,1 @@ Add enums to `SetTimer`
    -SetTimer("MyFunction", 2000, 0)
    +SetTimer("MyFunction", 2000, false)
```

The replace output will be:

```pawn
native bool:SetPlayerObjectMaterialText(playerid, objectid, text[], materialindex = 0, OBJECT_MATERIAL_SIZE:materialsize = OBJECT_MATERIAL_SIZE_256x128, fontface[] = "Arial", fontsize = 24, bold = 1, fontcolor = 0xFFFFFFFF, backcolor = 0, OBJECT_MATERIAL_TEXT_ALIGN:textalignment = OBJECT_MATERIAL_TEXT_ALIGN_LEFT);
SetTimer("MyFunction", 2000, false);
```

 Const Example
---------------

Given the following file:

```pawn
native SetPlayerObjectMaterialText(playerid, objectid, text[], materialindex = 0, materialsize = OBJECT_MATERIAL_SIZE_256x128, fontface[] = "Arial", fontsize = 24, bold = 1, fontcolor = 0xFFFFFFFF, backcolor = 0, textalignment = 0);
SetTimer("MyFunction", 2000, 0);
```

And parameters:

```
--scans const ../qawno/include
```

The report output will be:

```
Scanning file: D:\open.mp\upgrade\example.inc

    @@ -1,1 +1,1 @@ Add `const` to `SetPlayerObjectMaterialText`
    -native SetPlayerObjectMaterialText(playerid, objectid, text[],
    +native SetPlayerObjectMaterialText(playerid, objectid, const text[],
    @@ -1,1 +1,1 @@ Add `const` to `SetPlayerObjectMaterialText`
    -native SetPlayerObjectMaterialText(playerid, objectid, const text[], materialindex = 0, materialsize = OBJECT_MATERIAL_SIZE_256x128, fontface[] = "Arial"
    +native SetPlayerObjectMaterialText(playerid, objectid, const text[], materialindex = 0, materialsize = OBJECT_MATERIAL_SIZE_256x128, const fontface[] = "Arial"
```

The replace output will be:

```pawn
native SetPlayerObjectMaterialText(playerid, objectid, const text[], materialindex = 0, materialsize = OBJECT_MATERIAL_SIZE_256x128, const fontface[] = "Arial", fontsize = 24, bold = 1, fontcolor = 0xFFFFFFFF, backcolor = 0, textalignment = 0);
SetTimer("MyFunction", 2000, 0);
```

 Double Example
----------------

You can apply multiple replacements one after the other.  Combining the replacements above:

```
upgrade --scans const ../qawno/include
upgrade --scans tags ../qawno/include
```

Gives:

```pawn
native bool:SetPlayerObjectMaterialText(playerid, objectid, const text[], materialindex = 0, OBJECT_MATERIAL_SIZE:materialsize = OBJECT_MATERIAL_SIZE_256x128, const fontface[] = "Arial", fontsize = 24, bold = 1, fontcolor = 0xFFFFFFFF, backcolor = 0, OBJECT_MATERIAL_TEXT_ALIGN:textalignment = OBJECT_MATERIAL_TEXT_ALIGN_LEFT);
SetTimer("MyFunction", 2000, false);
```

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

 Regex
-------

This code uses regular expressions to perform the replacements.  Don't ask why.  Anyway, in order to support things like nested brackets it uses PCRE regex with *subroutines* in the searches, and `PCRE2_SUBSTITUTE_EXTENDED` in the replacements ([actually just one feature](https://stackoverflow.com/questions/34198247/multiple-replacement-with-just-one-regex), and that was re-implemented for this release).  The file `_define.json` is automatically included with every run and contains many pre-made definitions of various pawn constructs.  For example:

```json
		"binary": "0[bB][01]++(?:_[01]++)*+",
```

That is the full definition to match any binary number in pawn (`0b01001`, `0B0_1`, etc).  It uses `++` and `*+` for possessive matching to vastly reduce backtracking in the processing.  PCRE has a very high default backtracking limit of 10,000,00; but before making many of these subroutines possessive this was being exceeded regulary by failing matches.  The majority of these defines will not match leading and trailing spaces, so these must be dealt with in every search, the exception is `expression` because it deals with arbitrary internal whitespace in a lazy manner that makes it very greedy.  They are also all non-capturing, which is why some uses may copy these defines in to a match directly in order to capture a part of it (most commonly done with parameter names).

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

