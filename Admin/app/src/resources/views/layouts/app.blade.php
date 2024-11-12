<!DOCTYPE html>
<html lang="ja">
<head>
    <title>@yield('title')</title>
    <link href="/css/bootstrap.min.css" rel="stylesheet"
          integrity="sha384-9ndCyUaIbzAi2FUVXJi0CjmCapSmO7SnpJef0486qhLnuZ2cdeRhO02iuK6FUUVM" crossorigin="anonymous">
    <link rel="stylesheet" href="/bootstrap-icons/font/bootstrap-icons.css">
    @yield('style')
</head>
<body>
<nav class="navbar navbar-expand-md navbar-dark fixed-top bg-secondary">
    <div class="container-fluid">
        <a class="navbar-brand text-warning fw-bold fs-4" href="{{ route('home.index')}}">パズルゲーム管理サイト</a>
        <div class="collapse navbar-collapse" id="navbarCollapse">
            <ul class="navbar-nav me-auto mb-2 mb-md-0">
                {{--マスタデータ--}}
                <li>
                    <div class="dropdown">
                        <!-- 切替ボタンの設定 -->
                        <a class="btn btn-secondary dropdown-toggle" href="#" role="button" data-bs-toggle="dropdown"
                           aria-expanded="false">
                            マスタデータ
                        </a>
                        <!-- ドロップメニューの設定 -->
                        <ul class="dropdown-menu">
                            <li><a class="dropdown-item" href="{{ route('ngwords.index')}}">NGワード一覧表示</a></li>
                            <li><a class="dropdown-item" href="{{ route('constants.index')}}">定数一覧表示</a></li>
                            <li><a class="dropdown-item" href="{{ route('items.index')}}">アイテム一覧表示</a></li>
                            <li><a class="dropdown-item"
                                   href="{{ route('achievements.index')}}">アチーブメント一覧表示</a></li>
                            <li><a class="dropdown-item" href="{{ route('achievements.create')}}">アチーブメント作成</a>
                            </li>
                        </ul>
                    </div>
                </li>
                {{--ログデータ--}}
                <li>
                    <div class="dropdown">
                        <!-- 切替ボタンの設定 -->
                        <a class="btn btn-secondary dropdown-toggle" href="#" role="button" data-bs-toggle="dropdown"
                           aria-expanded="false">
                            ログデータ
                        </a>
                        <!-- ドロップメニューの設定 -->
                        <ul class="dropdown-menu">
                            <li><a class="dropdown-item" href="{{ route('logs.follow')}}">フォローログ一覧</a></li>
                            <li><a class="dropdown-item" href="{{ route('logs.item')}}">アイテムログ一覧</a></li>
                            <li><a class="dropdown-item" href="{{ route('logs.mail')}}">メールログ一覧</a></li>
                            <li><a class="dropdown-item" href="{{ route('logs.stageresult')}}">ステージリザルト一覧</a>
                            </li>
                        </ul>
                    </div>
                </li>
                {{--救難信号--}}
                <li>
                    <div class="dropdown">
                        <!-- 切替ボタンの設定 -->
                        <a class="btn btn-secondary dropdown-toggle" href="#" role="button" data-bs-toggle="dropdown"
                           aria-expanded="false">
                            救難信号
                        </a>
                        <!-- ドロップメニューの設定 -->
                        <ul class="dropdown-menu">
                            <li><a class="dropdown-item" href="{{ route('distresssignals.index')}}">一覧表示</a></li>
                        </ul>
                    </div>
                </li>
                {{--ユーザーデータ--}}
                <li>
                    <div class="dropdown">
                        <!-- 切替ボタンの設定 -->
                        <a class="btn btn-secondary dropdown-toggle" href="#" role="button" data-bs-toggle="dropdown"
                           aria-expanded="false">
                            ユーザーデータ
                        </a>
                        <!-- ドロップメニューの設定 -->
                        <ul class="dropdown-menu">
                            <li><a class="dropdown-item" href="{{ route('users.index')}}">一覧表示</a></li>
                            <li><a class="dropdown-item" href="{{ route('users.item')}}">所持アイテム</a></li>
                            <li><a class="dropdown-item" href="{{ route('users.follow')}}">フォロー一覧</a></li>
                            <li><a class="dropdown-item" href="{{ route('users.mail')}}">受信メール</a></li>
                            <li><a class="dropdown-item"
                                   href="{{ route('users.achievement')}}">アチーブメント達成状況</a></li>
                        </ul>
                    </div>
                </li>
                {{--アカウント管理--}}
                <li>
                    <div class="dropdown">
                        <!-- 切替ボタンの設定 -->
                        <a class="btn btn-secondary dropdown-toggle" href="#" role="button" data-bs-toggle="dropdown"
                           aria-expanded="false">
                            アカウント管理
                        </a>
                        <!-- ドロップメニューの設定 -->
                        <ul class="dropdown-menu">
                            <li><a class="dropdown-item" href="{{ route('accounts.index')}}">一覧表示</a></li>
                            <li><a class="dropdown-item" href="{{ route('accounts.create')}}">登録</a></li>
                        </ul>
                    </div>
                </li>
                {{--メール管理--}}
                <li>
                    <div class="dropdown">
                        <!-- 切替ボタンの設定 -->
                        <a class="btn btn-secondary dropdown-toggle" href="#" role="button" data-bs-toggle="dropdown"
                           aria-expanded="false">
                            メール管理
                        </a>
                        <!-- ドロップメニューの設定 -->
                        <ul class="dropdown-menu">
                            <li><a class="dropdown-item" href="{{ route('mails.index')}}">一覧表示</a></li>
                            <li><a class="dropdown-item" href="{{ route('mails.create')}}">送信</a></li>
                        </ul>
                    </div>
                </li>
            </ul>
            <button type="button" class="btn btn-outline-danger me-2"
                    onclick="location.href='{{ route('auths.dologout')}}'">
                ログアウト
            </button>
        </div>
    </div>
</nav>
@yield('body')
<script src="/js/bootstrap.bundle.min.js"
        integrity="sha384-geWF76RCwLtnZ8qwWowPQNguL3RmwHVBC9FhGdlKrxdiJJigb/j/68SIy3Te4Bkz"
        crossorigin="anonymous">
</script>
@yield('js')
</body>
</html>
