﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Tharga.Toolkit.Console;
using Tharga.Toolkit.Console.Command;
using Tharga.Toolkit.Console.Command.Base;

namespace SaintCoinach.Cmd.Commands {
    public class BgmCommand : ActionCommandBase {
        private ARealmReversed _Realm;

        public BgmCommand(ARealmReversed realm)
            : base("bgm", "Export all BGM files.") {
            _Realm = realm;
        }

        public override async Task<bool> InvokeAsync(string paramList) {
            var bgms = _Realm.GameData.GetSheet("BGM");

            var successCount = 0;
            var failCount = 0;
            foreach (Xiv.IXivRow bgm in bgms) {
                var filePath = (string)bgm["File"];

                try {

                    if (string.IsNullOrWhiteSpace(filePath))
                        continue;

                    IO.File file;
                    var fInfo = new System.IO.FileInfo(System.IO.Path.Combine(_Realm.GameVersion, filePath + ".ogg"));
                    if (_Realm.Packs.TryGetFile(filePath, out file)) {

                        if (!fInfo.Directory.Exists)
                            fInfo.Directory.Create();

                        var raw = file.GetData();
                        var decoder = new Sound.ScdDecoder(raw);
                        var decoded = decoder.GetData();

                        System.IO.File.WriteAllBytes(fInfo.FullName, decoded);

                        ++successCount;
                    } else {
                        OutputError("File {0} not found.", filePath);
                        try { if (fInfo.Exists) { fInfo.Delete(); } } catch { }
                        ++failCount;
                    }
                } catch(Exception e) {
                    OutputError("Export of {0} failed: {1}", filePath, e.Message);
                    ++failCount;
                }
            }
            OutputInformation("{0} files exported, {1} failed", successCount, failCount);

            return true;
        }
    }
}