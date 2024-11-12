@extends('layouts.app')
@section('title','IndexMail')
@section('body')
    <div class="container p-5">
        <div class="row">
            <div class="col-md-12">
                <h1 class="pt-5 pb-3">■ メール一覧</h1>
                @if(!empty($mailData))
                    {{$mailData->links('vendor.pagination.bootstrap-5')}}
                @endif
                <table class="table table-hover">
                    <thead class="table-primary">
                    <tr>
                        <th>ID</th>
                        <th>タイトル</th>
                        <th>テキスト</th>
                        <th>アイテム</th>
                        <th>生成日</th>
                        <th>最終更新日</th>
                    </tr>
                    </thead>
                    <tbody class="table-light">
                    @if(!empty($mailData))
                        @for($i = 0;$i < count($mailData);$i++)
                            <tr>
                                <td>{{$mailData[$i]['id']}}</td>
                                <td>{{$mailData[$i]['title']}}</td>
                                <td>{{$mailData[$i]['text']}}</td>
                                <td>{{$mailData[$i]['item']}}</td>
                                <td>{{$mailData[$i]['created_at']}}</td>
                                <td>{{$mailData[$i]['updated_at']}}</td>
                            </tr>
                        @endfor
                    @endif
                    </tbody>
                </table>
                <br>
            </div>
        </div>
    </div>
@endsection
