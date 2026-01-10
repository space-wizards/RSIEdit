{
  description = "C# GUI application for manipulation of RSI files used in SS14.";

  inputs = {
    nixpkgs.url = "github:NixOS/nixpkgs/release-25.05";
    self.submodules = true;
  };

  outputs = { self, nixpkgs }: {

    packages.x86_64-linux.rsiedit = nixpkgs.legacyPackages.x86_64-linux.callPackage (
      { lib
      , buildDotnetModule
      , fetchFromGitHub
      , dotnetCorePackages
      , libX11
      , libICE
      , libSM
      , libXi
      , libXcursor
      , libXext
      , libXrandr
      , fontconfig
      , glew
      , makeDesktopItem
      , copyDesktopItems
      , wrapGAppsHook
      , gtk3
      , pango
      , cairo
      , atk
      , zlib
      , glib
      , gdk-pixbuf
      }:

      let
        version = "0.7.1";
        pname = "rsiedit";
      in buildDotnetModule rec {
        inherit pname;
        inherit version;

        src = ./.;

        dotnet-sdk = dotnetCorePackages.sdk_9_0;
        dotnet-runtime = dotnetCorePackages.sdk_9_0;

        nugetDeps = ./deps.json; # File generated with `nix run .#fetch-deps`

        buildType = "Release";
        selfContainedBuild = false;
        
        projectFile = [
          "Editor/Editor.csproj"
        ];

        nativeBuildInputs = [ wrapGAppsHook copyDesktopItems ];
        buildInputs = [ gtk3 ];

        dontWrapGApps = true;

        preFixup = ''
          makeWrapperArgs+=("''${gappsWrapperArgs[@]}")
        '';

        runtimeDeps = [
            # Avalonia UI dependencies.
            libX11
            libICE
            libSM
            libXi
            libXcursor
            libXext
            libXrandr
            fontconfig
            glew
            
            # Needed for file dialogs.
            gtk3
            pango
            cairo
            atk
            zlib
            glib
            gdk-pixbuf
        ];

        executables = [ "Editor" ];

        desktopItems = [
          (makeDesktopItem {
            name = pname;
            exec = meta.mainProgram;
            desktopName = "RSIEdit";
            comment = meta.description;
            categories = [ "Graphics" ];
            startupWMClass = meta.mainProgram;
          })
        ];

        meta = with lib; {
          description = "C# GUI application for manipulation of RSI files used in SS14.";
          homepage = "https://github.com/space-wizards/RSIEdit";
          license = licenses.mit;
          platforms = [ "x86_64-linux" ];
          mainProgram = "Editor";
        };
      }) { };

    packages.x86_64-linux.fetch-deps = self.packages.x86_64-linux.rsiedit.passthru.fetch-deps;
    packages.x86_64-linux.default = self.packages.x86_64-linux.rsiedit;

    apps.x86_64-linux.rsiedit = {
      type = "app";
      program = "${self.packages.x86_64-linux.rsiedit}/bin/Editor";
    };

    apps.x86_64-linux.fetch-deps = {
      type = "app";
      program = "${self.packages.x86_64-linux.fetch-deps}";
    };

    apps.x86_64-linux.default = self.apps.x86_64-linux.rsiedit;

  };
}
