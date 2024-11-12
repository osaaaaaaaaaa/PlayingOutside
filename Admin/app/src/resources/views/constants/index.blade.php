@extends('layouts.app')
@section('title','ConstantList')
@section('body')
    <div class="container p-5">
        <div class="row">
            <div class="col-md-12">
                <h1 class="pt-5 pb-3">■ 定数一覧</h1>
                @if(!empty($constants))
                    {{$constants->onEachSide(2)->links('vendor.pagination.bootstrap-5')}}
                @endif
                <table class="table table-hover">
                    <thead class="table-primary">
                    <tr>
                        <th>ID</th>
                        <th>値</th>
                        <th>種別</th>
                    </tr>
                    </thead>
                    <tbody class="table-light">
                    @if(!empty($constants))
                        @foreach($constants as $data)
                            <tr>
                                <td>{{$data['id']}}</td>
                                <td>{{$data['constant']}}</td>
                                <td>{{$data['type']}}</td>
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
