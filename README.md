# Abstract refcounted game content manager

This small library does a very specific thing: it provides Monogame-inspired `ContentManager` which can load/unload abstract assets.
First load really loads the asset, then every load call increases reference count without doing anything.
Every unload call then decreases said count; when reference count hits zero asset is really unloaded.
 
As a bonus, provided thread-safe RefCounter can be used to count something else.