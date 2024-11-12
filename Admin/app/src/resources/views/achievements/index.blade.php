@extends('layouts.app')
@section('title','AchievementList')
@section('body')
    <div class="container p-5">
        <div class="row">
            <div class="col-md-12">
                <h1 class="pt-5 pb-3">■ アチーブメント一覧</h1>
                @if(!empty($achievements))
                    {{$achievements->onEachSide(2)->links('vendor.pagination.bootstrap-5')}}
                @endif
                <table class="table table-hover">
                    <thead class="table-primary">
                    <tr>
                        <th>ID</th>
                        <th>達成条件テキスト</th>
                        <th>種類</th>
                        <th>達成条件値</th>
                        <th>報酬アイテム</th>
                        <th>個数</th>
                    </tr>
                    </thead>
                    <tbody class="table-light">
                    @if(!empty($achievements))
                        @foreach($achievements as $data)
                            <tr>
                                <td>{{$data['id']}}</td>
                                <td>{{$data['text']}}</td>
                                <td>{{$data['type']}}</td>
                                <td>{{$data['achieved_val']}}</td>
                                <td>{{$data['item']}}</td>
                                <td>{{$data['item_amount']}}</td>
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
