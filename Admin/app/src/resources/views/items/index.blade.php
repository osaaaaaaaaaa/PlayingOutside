@extends('layouts.app')
@section('title','ItemList')
@section('body')
    <div class="container p-5">
        <div class="row">
            <div class="col-md-12">
                <h1 class="pt-5 pb-3">■ アイテム一覧</h1>
                @if(!empty($items))
                    {{$items->onEachSide(2)->links('vendor.pagination.bootstrap-5')}}
                @endif
                <table class="table table-hover">
                    <thead class="table-primary">
                    <tr>
                        <th>ID</th>
                        <th>アイテム名</th>
                        <th>種別</th>
                        <th>効果値</th>
                        <th>説明</th>
                    </tr>
                    </thead>
                    <tbody class="table-light">
                    @if(!empty($items))
                        @foreach($items as $data)
                            <tr>
                                <td>{{$data['id']}}</td>
                                <td>{{$data['name']}}</td>
                                <td>{{$data['type']}}</td>
                                <td>{{$data['effect']}}</td>
                                <td>{{$data['description']}}</td>
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
