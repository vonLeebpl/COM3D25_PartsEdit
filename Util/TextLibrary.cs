namespace CM3D2.PartsEdit.Plugin {
    // Napisy okna Library. Zrodlem jest japonski (jak w Text.cs); angielski jest w pliku tlumaczen COM3D25_PartsEdit.txt,
    // a Tr.T() podmienia napis, gdy plik jest dostepny. Szablony z {0}, {1} wypelnia string.Format.
    // Napisy "Name", "Rename", "Delete", "Delete set" i "Select a maid first" uzywaja kluczy, ktore juz istnieja w pliku.
    static class TL {
        public static readonly string LibraryUnavailable = Tr.T("ライブラリを利用できません（ログを参照してください）。");
        public static readonly string TabMaid = Tr.T("メイド");
        public static readonly string TabLibraryFmt = Tr.T("ライブラリ ({0})");
        public static readonly string TabSetsFmt = Tr.T("セット ({0})");
        public static readonly string SelectMaidFirst = Tr.T("先にメイドを選択してください。");

        public static readonly string AutoLoad = Tr.T("このメイドの読み込み時に自動適用");
        public static readonly string ApplyAll = Tr.T("すべて適用");
        public static readonly string ResetMaid = Tr.T("メイドをリセット");
        public static readonly string ResetDone = Tr.T("リセット: このメイドのPartsEditによる変更をすべて元に戻しました（手動編集も含む）。");
        public static readonly string SetsOfMaid = Tr.T("このメイドのセット");
        public static readonly string None = Tr.T("（なし）");
        public static readonly string Apply = Tr.T("適用");
        public static readonly string Revert = Tr.T("元に戻す");
        public static readonly string Edit = Tr.T("編集");
        public static readonly string AddSet = Tr.T("＋セットを追加");
        public static readonly string NoOtherSets = Tr.T("（他のセットはありません。セットタブで作成してください）");
        public static readonly string ModsOfMaid = Tr.T("このメイドの変更");
        public static readonly string AddMod = Tr.T("＋変更を追加");
        public static readonly string Search = Tr.T("検索");
        public static readonly string NoMatchingMods = Tr.T("（装着中のアイテムに合う変更はありません）");

        public static readonly string SaveSlotHeader = Tr.T("現在のスロットを変更として保存");
        public static readonly string ChooseSlot = Tr.T("（スロットを選択）");
        public static readonly string SlotFmt = Tr.T("スロット: {0}");
        public static readonly string SaveAsMod = Tr.T("変更として保存（このメイドにも割り当て）");
        public static readonly string ConvertBtn = Tr.T("オブジェクトのルート姿勢をボーン調整に変換");
        public static readonly string NothingEquipped = Tr.T("このスロットには何も装着されていません。");
        public static readonly string NothingToSave = Tr.T("保存するものがありません: このスロットのボーンはまだ変更されていません（先に編集するか、変更を適用してください）。");
        public static readonly string NothingToUpdate = Tr.T("更新するものがありません: このスロットのボーンは変更されていません。");
        public static readonly string RootConvertedPrefix = Tr.T("オブジェクトのルート姿勢をボーン調整に変換しました。");
        public static readonly string SavedFmt = Tr.T("「{0}」[{1}] を保存しました（{2}ボーン）。");
        public static readonly string UpdateBtnFmt = Tr.T("「{0}」を現在のスロットの状態で更新");
        public static readonly string SlotUnavailableFmt = Tr.T("「{0}」のスロットはこのメイドにありません。");
        public static readonly string UpdatedFmt = Tr.T("「{0}」を更新しました（{1}ボーン）。");
        public static readonly string EditingFmt = Tr.T("「{0}」を編集中。終了したら ライブラリ > メイド の「更新」を使ってください。");

        public static readonly string Sort = Tr.T("並べ替え");
        public static readonly string SortDate = Tr.T("日付");
        public static readonly string SortSlot = Tr.T("スロット");
        public static readonly string Asc = Tr.T("昇順");
        public static readonly string Desc = Tr.T("降順");
        public static readonly string LibraryEmpty = Tr.T("（ライブラリは空です）");
        public static readonly string MatchAny = Tr.T("スロット内のどのアイテムにも");
        public static readonly string MatchSame = Tr.T("同じアイテムのみ");
        public static readonly string ConfirmQ = Tr.T("確認？");
        public static readonly string ItemFmt = Tr.T("アイテム: {0}");
        public static readonly string RootPoseLabelFmt = Tr.T("オブジェクトのルート姿勢（回転 {0}度、移動 {1}cm）");
        public static readonly string RootOffRevertedFmt = Tr.T("「{0}」のルート姿勢をオフにして元に戻しました。");
        public static readonly string RootOnFmt = Tr.T("「{0}」のルート姿勢をオンにしました（適用してください）。");
        public static readonly string RootOffFmt = Tr.T("「{0}」のルート姿勢をオフにしました。");
        public static readonly string DeletedFmt = Tr.T("「{0}」を削除しました。");

        public static readonly string Rename = Tr.T("名前変更");
        public static readonly string Delete = Tr.T("削除");
        public static readonly string DeleteSet = Tr.T("セット削除");
        public static readonly string FilterThisMaid = Tr.T("このメイド");
        public static readonly string FilterAll = Tr.T("すべて");
        public static readonly string NoMineHint = Tr.T("（このメイド用の変更はまだありません。「すべて」で他の変更を確認できます）");
        public static readonly string OwnerFmt = Tr.T("作成者: {0}");
        public static readonly string NoOwner = Tr.T("（作成者不明）");
        public static readonly string ShowOthers = Tr.T("他のメイド用の変更も表示");
        public static readonly string ShowOtherSets = Tr.T("他のメイド用のセットも表示");
        public static readonly string NewSet = Tr.T("新規セット");
        public static readonly string Create = Tr.T("作成");
        public static readonly string NoSetsYet = Tr.T("（セットはまだありません）");
        public static readonly string Empty = Tr.T("（空）");
        public static readonly string AddModToSet = Tr.T("＋セットに変更を追加");
        public static readonly string NothingToAdd = Tr.T("（追加できるものはありません）");
        public static readonly string ApplyToMaid = Tr.T("メイドに適用");
        public static readonly string AssignToMaid = Tr.T("メイドに割り当て");
        public static readonly string UnassignFromMaid = Tr.T("メイドから割り当て解除");
        public static readonly string SetAssigned = Tr.T("セットをこのメイドに割り当てました。");
        public static readonly string SetUnassigned = Tr.T("セットの割り当てを解除し、元に戻しました。");
        public static readonly string ConfirmDeleteQ = Tr.T("削除を確認？");
        public static readonly string CreatedSetFmt = Tr.T("セット「{0}」を作成しました。");
        public static readonly string DeletedSetFmt = Tr.T("セット「{0}」を削除しました（含まれていた変更はライブラリに残ります）。");
        public static readonly string RevertedSetFmt = Tr.T("セット「{0}」を元に戻しました。");
        public static readonly string RevertedEntryFmt = Tr.T("「{0}」を元に戻しました。");
        public static readonly string SetRemovedFmt = Tr.T("セット「{0}」をこのメイドから外し、元に戻しました。");
        public static readonly string EntryRemovedFmt = Tr.T("「{0}」をこのメイドから外し、元に戻しました。");
        public static readonly string SetAddedFmt = Tr.T("セット「{0}」をこのメイドに追加しました。");

        // wyniki nakladania (LibraryService)
        public static readonly string AppliedFmt = Tr.T("適用しました: {0}/{1}。");
        public static readonly string ResultLineFmt = Tr.T("「{0}」[{1}]: {2}");
        public static readonly string StatusApplied = Tr.T("適用済み");
        public static readonly string StatusUnknownSlot = Tr.T("このメイドにそのスロットはありません");
        public static readonly string StatusSlotEmpty = Tr.T("スロットが空です");
        public static readonly string StatusNameMismatch = Tr.T("別のアイテムが装着されています（「同じアイテムのみ」の変更）");
        public static readonly string EnableIndependent = Tr.T("このスロットの「独立ボーン」を先にオンにしてください。");
        public static readonly string RootNotChanged = Tr.T("このスロットのルート姿勢は変更されていません（変換するものがありません）。");
        public static readonly string ConvertedFmt = Tr.T("オブジェクトのルート姿勢を{0}個のボーン調整に変換しました（ボーン基準）。保存するにはスロットを変更として保存してください。");
    }
}
