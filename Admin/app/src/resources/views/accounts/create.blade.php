@extends('layouts.app')
@section('title','CreateAccount')
@section('body')
    <div class="modal modal-sheet position-static d-block p-4 py-md-5" tabindex="-1" role="dialog">
        <div class="modal-dialog pt-5" role="document">
            <div class="modal-content rounded-4 shadow">
                <div class="modal-header p-5 pb-4 border-bottom-0">
                    <h1 class="fw-bold mb-0 fs-2">■ 管理者登録</h1>
                </div>
                <div class="modal-body p-5 pt-0">
                    <form method="post" action="{{ route('accounts.store')}}">
                        @csrf
                        <div class="form-floating mb-3">
                            <input type="text" class="form-control rounded-3" id="floatingInput"
                                   placeholder="アカウント名" name="account_name">
                            <label for="floatingInput">アカウント名</label>
                        </div>
                        <div class="form-floating mb-3">
                            <input type="password" class="form-control rounded-3" id="floatingPassword"
                                   placeholder="パスワード" name="password">
                            <label for="floatingPassword">パスワード</label>
                        </div>
                        <div class="form-floating mb-3">
                            <input type="password" class="form-control rounded-3" id="floatingPassword"
                                   placeholder="パスワード(確認用)" name="password_confirmation">
                            <label for="floatingPassword">パスワード(確認用)</label>
                        </div>
                        <button class="w-100 mb-2 btn btn-lg rounded-3 btn-primary" type="submit">登録</button>
                    </form>
                    <hr class="my-4">
                    @if($errors->any())
                        <ul>
                            @foreach($errors->all() as $text)
                                <li class="text-danger fw-bold">{{$text}}</li>
                            @endforeach
                        </ul>
                    @endif
                    @if(!empty($result))
                        <ul>
                            <li class="text-success fw-bold">{{$result}}</li>
                        </ul>
                    @endif
                    @if(!empty($error))
                        <ul>
                            <li class="text-danger fw-bold">{{$error}}</li>
                        </ul>
                    @endif

                </div>
            </div>
        </div>
    </div>
@endsection
