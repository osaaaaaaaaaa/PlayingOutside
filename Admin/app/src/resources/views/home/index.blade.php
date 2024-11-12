@extends('layouts.app')
@section('style')
    <style>
        .home-icon {
            font-size: 120px;
        }
    </style>
@endsection
@section('title','Home')
@section('body')
    <div class="container marketing pt-5 p-5">
        <!--マスタデータ管理 **** -->
        <h1 class="mt-5 mb-5 pb-2 border-bottom border-4">■ マスタデータ</h1>
        <div class="row d-flex flex-row justify-content-evenly" style="text-align:center">
            <div class="col-md-2 border rounded-3 bg-body-tertiary">
                <i class="bi bi-boxes home-icon"></i>
                <h4 class="fw-normal">NGワード一覧</h4>
                <p><a class="btn btn-primary" href="{{ route('ngwords.index')}}">View details »</a></p>
            </div>
            <div class="col-md-2 border rounded-3 bg-body-tertiary">
                <i class="bi bi-boxes home-icon"></i>
                <h4 class="fw-normal">定数一覧</h4>
                <p><a class="btn btn-primary" href="{{ route('constants.index')}}">View details »</a></p>
            </div>
            <div class="col-md-2 border rounded-3 bg-body-tertiary">
                <i class="bi bi-boxes home-icon"></i>
                <h4 class="fw-normal">アイテム一覧</h4>
                <p><a class="btn btn-primary" href="{{ route('items.index')}}">View details »</a></p>
            </div>
            <div class="col-md-2 border rounded-3 bg-body-tertiary">
                <i class="bi bi-boxes home-icon"></i>
                <h4 class="fw-normal">アチーブメント一覧</h4>
                <p><a class="btn btn-primary" href="{{ route('achievements.index')}}">View details »</a></p>
            </div>
            <div class="col-md-2 border rounded-3 bg-body-tertiary">
                <i class="bi bi-boxes home-icon"></i>
                <h4 class="fw-normal">アチーブメント作成</h4>
                <p><a class="btn btn-primary" href="{{ route('achievements.create')}}">View details »</a></p>
            </div>
        </div>
        <br>
        <!--ログデータ管理-->
        <h1 class="mt-5 mb-5 pb-2 border-bottom border-4">■ ログデータ</h1>
        <div class="row d-flex flex-row justify-content-evenly" style="text-align:center">
            <div class="col-md-2 border rounded-3 bg-body-tertiary">
                <i class="bi bi-database home-icon"></i>
                <h4 class="fw-normal">フォローログ一覧</h4>
                <p><a class="btn btn-primary" href="{{ route('logs.follow')}}">View details »</a></p>
            </div>
            <div class="col-md-2 border rounded-3 bg-body-tertiary">
                <i class="bi bi-database home-icon"></i>
                <h4 class="fw-normal">アイテムログ一覧</h4>
                <p><a class="btn btn-primary" href="{{ route('logs.item')}}">View details »</a></p>
            </div>
            <div class="col-md-2 border rounded-3 bg-body-tertiary">
                <i class="bi bi-database home-icon"></i>
                <h4 class="fw-normal">メールログ一覧</h4>
                <p><a class="btn btn-primary" href="{{ route('logs.mail')}}">View details »</a></p>
            </div>
            <div class="col-md-2 border rounded-3 bg-body-tertiary">
                <i class="bi bi-database home-icon"></i>
                <h4 class="fw-normal">ステージリザルト一覧</h4>
                <p><a class="btn btn-primary" href="{{ route('logs.stageresult')}}">View details »</a></p>
            </div>
        </div>
        <br>
        <!--救難信号データ管理 **** -->
        <h1 class="mt-5 mb-5 pb-2 border-bottom border-4">■ 救難信号データ管理</h1>
        <div class="row d-flex flex-row justify-content-evenly" style="text-align:center">
            <div class="col-md-2 border rounded-3 bg-body-tertiary">
                <i class="bi bi-clipboard-data home-icon"></i>
                <h4 class="fw-normal">救難信号一覧</h4>
                <p><a class="btn btn-primary" href="{{ route('distresssignals.index')}}">View details »</a></p>
            </div>
        </div>
        <br>
        <!--ユーザーデータ管理 **** -->
        <h1 class="mt-5 mb-5 pb-2 border-bottom border-4">■ ユーザーデータ管理</h1>
        <div class="pb-5 row d-flex flex-row justify-content-evenly" style="text-align:center">
            <div class="col-md-2 border rounded-3 bg-body-tertiary">
                <i class="bi bi-people-fill home-icon"></i>
                <h4 class="fw-normal">ユーザー一覧</h4>
                <p><a class="btn btn-primary" href="{{ route('users.index')}}">View details »</a></p>
            </div>
            <div class="col-md-2 border rounded-3 bg-body-tertiary">
                <i class="bi bi-handbag home-icon"></i>
                <h4 class="fw-normal">所持アイテム一覧</h4>
                <p><a class="btn btn-primary" href="{{ route('users.item')}}">View details »</a></p>
            </div>
            <div class="col-md-2 border rounded-3 bg-body-tertiary">
                <i class="bi bi-person-check-fill home-icon"></i>
                <h4 class="fw-normal">フォロー一覧</h4>
                <p><a class="btn btn-primary" href="{{ route('users.follow')}}">View details »</a></p>
            </div>
            <div class="col-md-2 border rounded-3 bg-body-tertiary">
                <i class="bi bi-mailbox home-icon"></i>
                <h4 class="fw-normal">受信メール一覧</h4>
                <p><a class="btn btn-primary" href="{{ route('users.mail')}}">View details »</a></p>
            </div>
            <div class="col-md-2 border rounded-3 bg-body-tertiary">
                <i class="bi bi-people-fill home-icon"></i>
                <h4 class="fw-normal">アチーブメントの達成状況一覧</h4>
                <p><a class="btn btn-primary" href="{{ route('users.achievement')}}">View details »</a></p>
            </div>
        </div>
        <br>
        <!--アカウント管理-->
        <h1 class="mt-5 mb-5 pb-2 border-bottom border-4">■ アカウント管理</h1>
        <div class="row d-flex flex-row justify-content-evenly" style="text-align:center">
            <div class="col-md-2 border rounded-3 bg-body-tertiary">
                <i class="bi bi-person-badge home-icon"></i>
                <h4 class="fw-normal">アカウント一覧</h4>
                <p><a class="btn btn-primary" href="{{ route('accounts.index')}}">View details »</a></p>
            </div>
            <div class="col-md-2 border rounded-3 bg-body-tertiary">
                <i class="bi bi-person-plus-fill home-icon"></i>
                <h4 class="fw-normal">アカウント登録</h4>
                <p><a class="btn btn-primary" href="{{ route('accounts.create')}}">View details »</a></p>
            </div>
        </div>
        <br>
        <!--メール管理-->
        <h1 class="mt-5 mb-5 pb-2 border-bottom border-4">■ メール管理</h1>
        <div class="row d-flex flex-row justify-content-evenly" style="text-align:center">
            <div class="col-md-2 border rounded-3 bg-body-tertiary">
                <i class="bi bi-envelope-open home-icon"></i>
                <h4 class="fw-normal">メール一覧</h4>
                <p><a class="btn btn-primary" href="{{ route('mails.index')}}">View details »</a></p>
            </div>
            <div class="col-md-2 border rounded-3 bg-body-tertiary">
                <i class="bi bi-envelope home-icon"></i>
                <h4 class="fw-normal">メール送信</h4>
                <p><a class="btn btn-primary" href="{{ route('mails.create')}}">View details »</a></p>
            </div>
        </div>
    </div>
@endsection
