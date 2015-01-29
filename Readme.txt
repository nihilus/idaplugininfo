
This is a little utility to dump information about installed IDA Pro plugins.
The key parts of the IDA PLUGIN export "plugin_t" struct data.

Use it to glean information about what plugins you have.
See what the default hotkeys are, the short names, comments, version,
and their flags.
By comparing the flags (see "loader.hpp" in the IDA SDK) you can see which
plugins stay resident for example.

Note with the default IDA install you'll have about 50 plugins installed,
or close to 100 if considering the EA64 ".p64" versions too.

I made this because I was curious about which plugins I had, to look for
potential hotkey conflicts, and to help me move rarely used plugins
to a archive folder to slightly speed up IDA Pro load times (in particular
while debugging my own plugins).

To use it just run it and select your "plugins" folder off of your IDA Pro
installation folder (and, or, if you have a "plugs_archive" type folder too
like I have).

More of my work, blog, forum @ www.macromonkey.com
