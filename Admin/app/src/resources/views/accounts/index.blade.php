@extends('layouts.app')
@section('title','AccountsList')
@section('body')
    <div class="container p-5">
        <div class="row">
            <div class="col-md-12">
                <h1 class="pt-5 pb-3">■ アカウント一覧</h1>
                <form class="d-flex pb-3" role="search" method="get" action="{{route('accounts.show')}}">
                    @csrf
                    <div class="row">
                        <div class="col-md-8">
                            <input name="id" class="form-control me-2 border-3" type="search"
                                   placeholder="アカウントIDを入力"
                                   aria-label="Search">
                        </div>
                        <div class="col-md-4">
                            <button class="btn btn-success btn-gradient border-2" type="submit">検索</button>
                        </div>
                    </div>
                </form>
                <table class="table table-hover">
                    <thead class="table-primary">
                    <tr>
                        <th>ID</th>
                        <th>ユーザー名</th>
                        <th>パスワード</th>
                        <th>操作</th>
                    </tr>
                    </thead>
                    <tbody class="table-light">
                    @if(!empty($accounts))
                        @foreach($accounts as $data)
                            <tr>
                                <td>{{$data['id']}}</td>
                                <td>{{$data['name']}}</td>
                                <td>{{$data['password']}}</td>
                                <td>
                                    <div class="row">
                                        <div class="col-md-5">
                                            <button class="btn btn-danger btn-gradient border-2 delButton"
                                                    type="submit"
                                                    data-bs-toggle="modal" data-bs-target="#modalDestroy"
                                                    data-account_name="{{$data['name']}}"
                                                    data-account_id="{{$data['id']}}">
                                                削除
                                            </button>
                                        </div>
                                        <div class="col-md-5">
                                            <button class="btn btn-success btn-gradient border-2 udButton"
                                                    type="submit"
                                                    data-bs-toggle="modal" data-bs-target="#modalUpdate"
                                                    data-account_name="{{$data['name']}}"
                                                    data-account_id="{{$data['id']}}">
                                                更新
                                            </button>
                                        </div>
                                    </div>
                                </td>
                            </tr>
                        @endforeach
                    @endif
                    </tbody>
                </table>
                <br>
            </div>
        </div>
    </div>

    <!-- 削除用のモーダル -->
    <div class="modal fade" id="modalDestroy" tabindex="-1" aria-labelledby="exampleModalLabel" aria-hidden="false">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h1 class="modal-title fs-5" id="modalDestroyLabel">* アカウント削除</h1>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body">
                    <label id="delModalLabel">データを削除しますか？</label>
                </div>
                <div class="modal-footer">
                    <form method="post" action="{{route('accounts.destroy')}}">
                        @csrf
                        <button type="submit" class="btn btn-danger">削除</button>
                        <input type="hidden" id="destroy_account_id" name="destroy_account_id" value="">
                    </form>
                    <button type="button" class="btn btn-default" data-bs-dismiss="modal" aria-label="Close">閉じる
                    </button>
                </div>
            </div>
        </div>
    </div>

    <!-- 更新用のモーダル -->
    <div class="modal fade" id="modalUpdate" tabindex="-1">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h1 class="modal-title fs-5" id="modalDestroyLabel">* パスワード更新</h1>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body">
                    <form id="update" method="post" action="{{ route('accounts.update')}}">
                        @csrf
                        <div class="form-floating mb-3">
                            <input type="text" class="form-control rounded-3" id="floatingName"
                                   placeholder="アカウント名" value="aaa" disabled>
                            <label for="floatingInput">アカウント名</label>
                        </div>
                        <div class="form-floating mb-3">
                            <input type="password" class="form-control rounded-3" id="floatingPassword"
                                   placeholder="パスワード" name="password">
                            <label for="floatingPassword">パスワード</label>
                        </div>
                        <div class="form-floating mb-3">
                            <input type="password" class="form-control rounded-3" id="floatingPassword_confirmation"
                                   placeholder="パスワード(確認用)" name="password_confirmation">
                            <label for="floatingPassword">パスワード(確認用)</label>
                        </div>
                        <input type="hidden" id="update_account_id" name="update_account_id" value="">
                    </form>
                </div>
                <div class="modal-footer">
                    <button form="update" class="w-100 mb-2 btn rounded-3 btn-primary" type="submit">更新
                    </button>
                </div>
            </div>
        </div>
    </div>

    {{--トースト（プッシュ通知）のコンテナ--}}
    <div class="toast-container position-fixed bottom-0 end-0 p-3" id="toast-container">
    </div>

@endsection
@section('js')
    <script type="module">
        //================================
        // モーダルにパラメータを渡す(削除用)
        //================================

        const delButtons = document.getElementsByClassName('delButton');    // テーブルにある削除ボタンを取得する
        const delModalBody = document.getElementById('delModalLabel');      // 削除用モーダルのラベル
        const delModalHiddenVal = document.getElementById('destroy_account_id');   // 削除用モーダルのhiddenを取得

        // テーブルにある削除ボタンのイベント
        for (let i = 0; i < delButtons.length; i++) {
            delButtons[i].addEventListener('click', (e) => {
                //---------------------------
                // それぞれにカスタムデータ属性を代入する
                //---------------------------

                // ラベル更新
                delModalBody.textContent = delButtons[i].dataset.account_name + 'を削除しますか？';
                // hiddenの値を更新
                delModalHiddenVal.value = delButtons[i].dataset.account_id;
            });
        }

        //================================
        // モーダルにパラメータを渡す(更新用)
        //================================

        const udButtons = document.getElementsByClassName('udButton');    // テーブルにある削除ボタンを取得する
        const udModalInputName = document.getElementById('floatingName');      // 更新用モーダルのinput(text)
        const udModalHiddenVal = document.getElementById('update_account_id');   // 削除用モーダルのhiddenを取得

        // テーブルにある更新ボタンのイベント
        for (let i = 0; i < udButtons.length; i++) {
            udButtons[i].addEventListener('click', (e) => {
                //---------------------------
                // それぞれにカスタムデータ属性を代入する
                //---------------------------

                console.log(delButtons[i].dataset.account_name);

                // ラベル更新
                udModalInputName.value = delButtons[i].dataset.account_name;
                // hiddenの値を更新
                udModalHiddenVal.value = delButtons[i].dataset.account_id;
            });
        }

        //======================================
        // 更新・削除処理の後にトーストを焼く(プッシュ通知)
        //======================================

        function createToast(text, isError) {

            // 下準備
            const toastContainer = document.getElementById('toast-container');
            const title = isError ? 'バリデーションエラー' : 'リクエスト結果';
            const color = isError ? 'red' : 'green';
            const fontColor = isError ? 'text-danger' : 'text-success';
            const img = isError ? 'bi-exclamation-triangle-fill' : 'bi-check-lg';

            // トースト作成
            let toastLive = document.createElement('div');
            toastLive.classList.add("toast");
            toastLive.role = "alert";
            toastLive.ariaLive = "assertive";
            toastLive.ariaAtomic

            toastLive.innerHTML = '' +
                '<div class="toast-header">' +
                '<i class="bi ' + img + ' rounded me-2" style="color: ' + color + '"></i>' +
                '<strong class="me-auto ' + fontColor + ' fw-bold">' + title + '</strong>' +
                '<button type="button" class="btn-close" data-bs-dismiss="toast" aria-label="Close"></button>' +
                '</div>' +
                '<div class="toast-body">' + text + '</div>';

            // 作成したトーストをコンテナに追加
            toastContainer.appendChild(toastLive);

            return toastLive;
        }

        let toastLive_error;
        let toast_error;
        let toastLive_normally;
        let toast_normally;
        //----------------------------
        //  更新に失敗した場合
        //----------------------------
        @if($errors->any())
        @foreach($errors->all() as $text)

        // トーストを作成
        toastLive_error = createToast('{{$text}}', true);

        // トーストを表示する
        toast_error = new bootstrap.Toast(toastLive_error);
        toast_error.show();
        @endforeach
        @endif
        //----------------------------
        //  更新に成功した場合
        //----------------------------
        @if(!empty($normally))

        // トーストを作成
        toastLive_normally = createToast('パスワード更新完了', false);

        // トーストを表示する
        toast_normally = new bootstrap.Toast(toastLive_normally);
        toast_normally.show();

        @endif
        //----------------------------
        //  削除に失敗した場合
        //----------------------------
        @if(!empty($destroy_error))

        // トーストを作成
        toastLive_error = createToast('アカウントの削除に失敗しました', true);

        // トーストを表示する
        toast_error = new bootstrap.Toast(toastLive_error);
        toast_error.show();

        @endif
        //----------------------------
        //  削除に成功した場合
        //----------------------------
        @if(!empty($destroy_normally))

        // トーストを作成
        toastLive_normally = createToast('アカウントの削除に成功しました', false);

        // トーストを表示する
        toast_normally = new bootstrap.Toast(toastLive_normally);
        toast_normally.show();

        @endif
    </script>
@endsection
