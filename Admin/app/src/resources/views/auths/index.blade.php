<!DOCTYPE html>
<html lang="ja">
<head>
    <title>Login</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet"
          integrity="sha384-9ndCyUaIbzAi2FUVXJi0CjmCapSmO7SnpJef0486qhLnuZ2cdeRhO02iuK6FUUVM" crossorigin="anonymous">
</head>
<body class="bg-body-secondary">
<div class="modal modal-sheet position-static d-block p-4 py-md-5" tabindex="-1" role="dialog">
    <div class="modal-dialog" role="document">
        <div class="modal-content rounded-4 shadow">
            <div class="modal-header p-5 pb-4 border-bottom-0">
                <h1 class="fw-bold mb-0 fs-2">ログイン情報を入力</h1>
            </div>
            <div class="modal-body p-5 pt-0">
                <form method="post" action="{{ route('auths.dologin')}}">
                    @csrf
                    <div class="form-floating mb-3">
                        <input type="text" name="name" value="@if(!empty($name)){{$name}}@endif"
                               class="form-control rounded-3"
                               id="floatingInput"
                               placeholder="ユーザー名:">
                        <label for="floatingInput">アカウント名</label>
                    </div>
                    <div class="form-floating mb-3">
                        <input type="password" name="password" class="form-control rounded-3" id="floatingPassword"
                               placeholder="パスワード:">
                        <label for="floatingPassword">パスワード</label>
                    </div>
                    <button class="w-100 mb-2 btn btn-lg rounded-3 btn-primary" type="submit">ログイン</button>
                    <label><input hidden="hidden" name="action" value="doLogin"></label>
                </form>
                @if($errors->any())
                    <ul>
                        @foreach($errors->all() as $text)
                            <li class="text-danger fw-bold">{{$text}}</li>
                        @endforeach
                    </ul>
                @endif
                @if(!empty($error))
                    <ul>
                        <li class="text-danger fw-bold">"パスワード"または"アカウント名"が異なります</li>
                    </ul>
                @endif
            </div>
        </div>
    </div>
</div>
<script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"
        integrity="sha384-geWF76RCwLtnZ8qwWowPQNguL3RmwHVBC9FhGdlKrxdiJJigb/j/68SIy3Te4Bkz"
        crossorigin="anonymous"></script>
</body>
</html>
