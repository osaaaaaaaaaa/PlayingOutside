@extends('layouts.app')
@section('title','NGWordList')
@section('body')
    <div class="container p-5">
        <div class="row">
            <div class="col-md-12">
                <h1 class="pt-5 pb-3">■ NGワード一覧</h1>
                @if(!empty($ngwords))
                    {{$ngwords->onEachSide(2)->links('vendor.pagination.bootstrap-5')}}
                @endif
                <table class="table table-hover">
                    <thead class="table-primary">
                    <tr>
                        <th>ID</th>
                        <th>禁止ワード</th>
                    </tr>
                    </thead>
                    <tbody class="table-light">
                    @if(!empty($ngwords))
                        @foreach($ngwords as $data)
                            <tr>
                                <td>{{$data['id']}}</td>
                                <td>{{$data['word']}}</td>
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
