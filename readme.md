This program attempts to find old untagged and const-incorrect code and upgrade it.  There are many pre-defined upgrade replacements and you can create your own either manually or automatically.

 Usage
-------

To replace most native declarations and hooks with const-correct equivalents use:

```
./upgrade --scans const ../qawno/include
```

This will load all the replacements from `./const.json` and apply them to every file recursively in `../qawno/include`.

If you don't want to apply the changes immediately use:

```
./upgrade --report --scans const ../qawno/include
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
./upgrade --scans const ../qawno/include
./upgrade --scans tags ../qawno/include
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
./upgrade --generate
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

 Writing Replacements
----------------------

The core of the replacement system is regex ([yes, I know](https://xkcd.com/1171/)), this may seem inadequate for most situations, but it uses a lot of pre-defined procedures to enable things like matching a parameter, a declaration, or even an entire nested expression.  Fortunately pawn is fully deterministic in its parsing, so the scanners disable backtracking so that match failures don't explode.  Most of these procedures are also tightly matching, so `&symbol` will match any properly named symbol (starts with a letter, `_`, or `@`; and followed by any of those or a number), but will not consume any trailing whitespace.  Thus the majority of the scanners will use `\s*+` a lot (non-backtracking whitespace).  The pre-defined procedures are:

* `&symbol` - Any properly named pawn symbol (defines, functions, variables, keywords, etc).
* `&prefix` - The first half of a method-like function, such as `Command_` in `Command_GetName`.
* `&float` - A floating-point number, including `-`, the `_` separator, and exponents.
* `&hex` - A hexadecimal number, including the `_` separator.
* `&binary` - A binary number, including the `_` separator.
* `&integer` - An integer, including `-` and the `_` separator.
* `&character` - A character literal, with special handling of `'\''` and `'\\'`.
* `&string` - A string literal, with special handling of `"\""` and `"\\"`.
* `&operator` - Any of the pawn operators such as `*` or `>>>=`.
* `&number` - Any type of number - float, binary, hex, or decimal.
* `&tag` - One or more tags, so will match `File:` or `{Float, _}:`.
* `&varargs` - `...` at the end of a function declaration, optionally with a tag.
* `&expressionpart` - Used internally by the other expression procedures.
* `&expression` - A complete expression, ended by either `,`, `;`, or an unmatched closing bracket of some type.  Fully handles nested expressions so `(a, b)` can be matched as a *single* expression when being given as, say, a parameter to a function.  This is often the most useful procedure to use when matching code calling your functions to be replaced.  Although it theoretically encompasses many of the above procedures such as numbers and symbols it uses shortcuts by just matching any string of numbers, letters, and operators; without actually caring if the resulting code is technically valid.  Note that while most other procedures do not consume trailing space this one does because it uses some lazy techniques to more quickly match things that look vaguely like expressions.
* `&squarebrackets` - An expression between `[]`s.  Expressions are ended by unmatched brackets, so this consumes the first one, then uses `&expression`, then checks for the closing one.  May contain multiple expressions separated by `,` or `;` within the brackets.
* `&roundbrackets` - An expression between `()`s.  Expressions are ended by unmatched brackets, so this consumes the first one, then uses `&expression`, then checks for the closing one.  May contain multiple expressions separated by `,` or `;` within the brackets.
* `&curlybrackets` - An expression between `{}`s.  Expressions are ended by unmatched brackets, so this consumes the first one, then uses `&expression`, then checks for the closing one.  May contain multiple expressions separated by `,` or `;` within the brackets.
* `&anglebrackets` - An expression between `<>`s.  Expressions are ended by unmatched brackets, so this consumes the first one, then uses `&expression`, then checks for the closing one.  May contain multiple expressions separated by `,` or `;` within the brackets.  This one is for special arrays, but has not been fully tested in some situations for cases like `new special:array<5 < 6>;`, just don't do that!
* `&parameter` - Any parameter declaration in a function declaration.  This is more restrictive than a full expression as it must resemble `[const] [&][Tag:]name[size][= default]`; however, both array sizes and default values may be full expressions.
* `&const` - Like `&parameter`, but must include `const`.
* `&nonconst` - Like `&parameter`, but must not include `const`.
* `&untagged` - Like `&parameter`, but must not include a tag, `const`, or an array size.
* `&parameterlist` - All the parameters in a function declaration.  Includes the `()`s surrounding them, then naught or more parameters, optionally ending with `...`.
* `&declaration` - A symbol with a tag.
* `&publics` - A collection of keywords known to be used to declare public functions.
* `&stocks` - A collection of keywords known to be used to declare normal functions.
* `&start` - The start of a line, including any leading space.

