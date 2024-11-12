@extends('layouts.app')
@section('title','UserAchievementList')
@section('body')
    <div class="container p-5">
        <div class="row">
            <div class="col-md-12">
                <h1 class="pt-5 pb-3">■ アチーブメントの達成状況一覧</h1>
                <form class="d-flex pb-3" role="search" method="get"
                      action="{{ route('users.achievement.show')}}">
                    @csrf
                    <div class="row">
                        <div class="col-md-8">
                            <input name="id" class="form-control me-2 border-3" type="search"
                                   placeholder="ユーザーIDを入力"
                                   aria-label="Search">
                        </div>
                        <div class="col-md-4">
                            <button class="btn btn-success btn-gradient border-2" type="submit">検索</button>
                        </div>
                    </div>
                </form>
                @if(!empty($achievements))
                    {{$achievements->onEachSide(2)->links('vendor.pagination.bootstrap-5')}}
                @endif
                <table class="table table-hover">
                    <thead class="table-primary">
                    <tr>
                        <th>ID</th>
                        <th>ユーザー名</th>
                        <th>アチーブメントID</th>
                        <th>進捗値</th>
                        <th>達成したかどうか</th>
                        <th>更新日</th>
                    </tr>
                    </thead>
                    <tbody class="table-light">
                    @if(!empty($achievements))
                        @foreach($achievements as $achievement)
                            <tr>
                                <td>{{$achievement->id}}</td>
                                <td>{{$user->name}}</td>
                                <td>{{$achievement->achievement_id}}</td>
                                <td>{{$achievement->progress_val}}</td>
                                <td>{{$achievement->is_achieved}}</td>
                                <td>{{$achievement->updated_at}}</td>
                            </tr>
                        @endforeach
                    @endif
                    </tbody>
                </table>
                <br>
            </div>
        </div>
    </div>
@endsection
