# Monogame custom content pipeline tools

Quickstart:

	// Teach content managers to load custom asset type, named 'BMFont'.
	// It uses another asset, 2D texture with glyphs.
	CustomContentPipeline.RegisterLoad<BMFont>((content, assetName) => {
        string fontTextureName = FontData.GetTextureNameForFont(assetName);
        var fontTexture = content.Load<Texture2D>(fontTextureName);

        using (var stream = CustomContentPipeline.OpenTitleStorage(Game.ContentRoot, assetName)) {
            var fontDesc = FontData.Load(stream);
            return new BMFont(fontTexture, fontDesc);
        }
    });

	// Optional unload step to unload linked assets, in this example font uses texture with glyphs:
    CustomContentPipeline.RegisterUnload<BMFont>((content, asset, assetName) => {
        string fontTextureName = FontData.GetTextureNameForFont(assetName);
        content.Unload(fontTextureName);
    });

	// Then:

	 Content = new SharedContentManager(Game.Services, Game.ContentRoot);
	 var bmFont = Content.Load<BMFont>("Fonts\\Calibri22.fnt");