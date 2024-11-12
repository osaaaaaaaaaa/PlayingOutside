@extends('layouts.app')
@section('title','CreateAchievement')
@section('body')
    <div class="container my-3">
        <main>
            <div class="py-5 pt-5">
                <h1 class="pt-5">■ アチーブメント作成</h1>
            </div>
            <div class="row">
                <div>
                    <form class="needs-validation" method="get" action="{{ route('achievements.store')}}">
                        @csrf
                        <br>
                        <div class="col-md-12">
                            <label for="text" class="form-label">達成条件のテキスト</label>
                            <textarea id="textarea" class="form-control" name="text"
                                      placeholder="テキストを入力" required></textarea>
                        </div>
                        <br>
                        <div class="col-md-12">
                            <div class="row py-3 mb-2 item_ele">
                                <div class="col-md-4">
                                    <label for="state" class="form-label">アチーブメントの種類</label>
                                    <select class="form-select" id="state" name="type" required>
                                        @if(!empty($type))
                                            @for($i = 0;$i < count($type);$i++)
                                                <option value="{{$i + 1}}">{{$type[$i]['name']}}</option>
                                            @endfor
                                        @endif
                                    </select>
                                </div>
                                <div class="col-md-2">
                                    <label for="number" class="form-label">達成条件値</label>
                                    <input type="number" id="number" name="achieved_val" class="form-control"
                                           min="1"
                                           value="1" required>
                                </div>
                            </div>
                        </div>
                        <div class="col-md-12">
                            {{--アイテムの種類とアイテムの個数--}}
                            <div id="item_container">
                                <div class="row py-3 mb-2 item_ele">
                                    <div class="col-md-4">
                                        <label for="state" class="form-label">報酬アイテムの種類</label>
                                        <select class="form-select" id="state" name="item_id" required>
                                            @if(!empty($items))
                                                @foreach($items as $item)
                                                    <option value="{{$item->id}}">{{$item->name}}</option>
                                                @endforeach
                                            @endif
                                        </select>
                                    </div>
                                    <div class="col-md-2">
                                        <label for="number" class="form-label">報酬アイテムの個数</label>
                                        <input type="number" id="number" name="item_amount" class="form-control"
                                               min="1"
                                               value="1" required>
                                    </div>
                                </div>
                            </div>
                            <br>
                        </div>

                        <hr class="my-4">
                        @if($errors->any())
                            <ul>
                                @foreach($errors->all() as $text)
                                    <li class="text-danger fw-bold">{{$text}}</li>
                                @endforeach
                            </ul>
                        @endif
                        @if(!empty($normally))
                            <ul>
                                <li class="text-success fw-bold">アチーブメントを作成しました。</li>
                            </ul>
                        @endif

                        <input type="hidden" id="item_data" name="item_data">
                        <button class="w-100 btn btn-primary btn-lg my-3 pt-2" type="submit">作成</button>
                    </form>
                </div>
            </div>
        </main>
    </div>
@endsection
