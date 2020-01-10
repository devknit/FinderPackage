# Finder

Unity が管理しているアセットを見つける補助ツールです。

## インストール方法


### PackageManagerを使用する場合

Packages/manifest.json に以下を追加してください。

```
"com.devknit.finder": "https://github.com/devknit/FinderPackage.git"
```
※gitのコマンドが実行可能な状態で行って下さい。

パッケージの更新を行う場合は Packages/manifest.json の  
"lock" ノードの対象パッケージに関する記述を削除してください。

### PackageManagerを使用しない場合

Editor フォルダを Assets以下に配置してください。

## 起動方法

Unity のメニューに Tools > Finder > Open から起動できます。
ショートカットキーは　ALT + F になります。

