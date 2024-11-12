@extends('layouts.app')
@section('title','UserList')
@section('body')
    <div class="container p-5">
        <div class="row">
            <div class="col-md-12">
                <h1 class="pt-5 pb-3">■ ユーザー一覧</h1>
                <form class="d-flex pb-3" role="search" method="get"
                      action="{{ route('users.index.show')}}">
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
                @if(!empty($userData) && empty($requestID))
                    {{$userData->onEachSide(2)->links('vendor.pagination.bootstrap-5')}}
                @endif
                <table class="table table-hover">
                    <thead class="table-primary">
                    <tr>
                        <th>ID</th>
                        <th>ユーザー名</th>
                        <th>称号</th>
                        <th>トータルスコア</th>
                        <th>最新のステージ</th>
                    </tr>
                    </thead>
                    <tbody class="table-light">
                    @if(!empty($userData))
                        @foreach($userData as $data)
                            <tr>
                                <td>{{$data['id']}}</td>
                                <td>{{$data['name']}}</td>
                                <td>{{$data['title']}}</td>
                                <td>{{$data['total_score']}}</td>
                                <td>{{$data['stage_id']}}</td>
                            </tr>
                        @endforeach
                    @endif
                    {{--                    @if(!empty($user))
                                            <tr>
                                                <td>{{$users->id}}</td>
                                                <td>{{$users->name}}</td>
                                                <td>{{$title == null ? '' : $title}}</td>
                                                <td>{{$total_score == null ? 0 : $total_score}}</td>
                                                <td>{{$user->stage_id}}</td>
                                            </tr>
                                        @endif--}}
                    </tbody>
                </table>
                <br>
            </div>
        </div>
    </div>
@endsection
