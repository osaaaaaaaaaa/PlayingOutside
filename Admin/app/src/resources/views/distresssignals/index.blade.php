@extends('layouts.app')
@section('title','DistressSignalList')
@section('body')
    <div class="container p-5">
        <div class="row">
            <div class="col-md-12">
                <h1 class="pt-5 pb-3">■ 救難信号一覧</h1>
                <form class="d-flex pb-3" role="search" method="get"
                      action="{{ route('distresssignals.index.show')}}">
                    @csrf
                    <div class="row">
                        <div class="col-md-8">
                            <input name="action" class="form-control me-2 border-3" type="search"
                                   placeholder="0:挑戦中,1:ゲームクリア"
                                   aria-label="Search">
                        </div>
                        <div class="col-md-4">
                            <button class="btn btn-success btn-gradient border-2" type="submit">検索</button>
                        </div>
                    </div>
                </form>
                @if(!empty($users))
                    {{$users->onEachSide(2)->links('vendor.pagination.bootstrap-5')}}
                @endif
                <table class="table table-hover">
                    <thead class="table-primary">
                    <tr>
                        <th>ID</th>
                        <th>ホスト名</th>
                        <th>ゲスト</th>
                        <th>ステージID</th>
                        <th>アクション</th>
                        <th>生成日</th>
                        <th>更新日</th>
                    </tr>
                    </thead>
                    <tbody class="table-light">
                    @if(!empty($distresssignals))
                        @foreach($distresssignals as $signal)
                            <tr>
                                <td>{{$signal['id']}}</td>
                                <td>{{$signal['host']}}</td>
                                <td>{{$signal['guests']}}</td>
                                <td>{{$signal['stage_id']}}</td>
                                <td>{{$signal['action']}}</td>
                                <td>{{$signal['created_at']}}</td>
                                <td>{{$signal['updated_at']}}</td>
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
