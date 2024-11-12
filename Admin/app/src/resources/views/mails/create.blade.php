@extends('layouts.app')
@section('title','CreateMail')
@section('body')
    <div class="container my-3">
        <main>
            <div class="py-5 pt-5 ">
                <h1 class="pt-5">■ メール送信</h1>
            </div>
            <div class="row">
                <div>
                    <form class="needs-validation" method="post" action="{{ route('mails.store')}}">
                        @csrf
                        <label for="target"
                               class="form-label">送信先のユーザーIDを入力(全ユーザー指定のIDは0)</label>
                        <div class="col-md-2 py-1">
                            <input type="number" id="target" name="target_id" class="form-control" min="0" max="<?php
                                if (!empty($userMax)) {
                                    echo $userMax;
                                }else{
                                    echo 0;
                                }
                                ?>" value="0" required>
                        </div>
                        <br>
                        <div class="col-md-12">
                            <label for="title" class="form-label">タイトル</label>
                            <textarea id="textarea" class="form-control" name="title"
                                      placeholder="タイトルを入力" required></textarea>
                        </div>
                        <br>
                        <div class="col-md-12">
                            <label for="text" class="form-label">テキスト</label>
                            <textarea id="textarea" class="form-control" name="text"
                                      placeholder="テキストを入力" required></textarea>
                        </div>
                        <br>
                        <div class="col-md-2 py-1">
                            <label for="number" class="form-label">アイテムの種類数</label>
                            <input type="number" id="type_cnt" name="type_cnt" class="form-control" min="0"
                                   value="0" required <?php
                                                      if (empty($items)) {
                                                          echo 'disabled';
                                                      }
                                                      ?>>
                        </div>
                        <br>

                        <div class="collapse" id="collapseItems">

                            <hr class="my-1 pb-3">

                            {{--アイテムの種類とアイテムの個数--}}
                            <h4 class="mb-3">アイテム情報を設定</h4>
                            <div id="item_container">
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
                                <li class="text-success fw-bold">メールを送信しました</li>
                            </ul>
                        @endif

                        <input type="hidden" id="item_data" name="item_data">
                        <button class="w-100 btn btn-primary btn-lg my-3 pt-2" type="submit">送信</button>
                    </form>
                </div>
            </div>
        </main>
    </div>
@endsection
@section('js')
    <script>

        //==================================================
        // アイテムの種類数の値に応じてアイテムのフォームをセットする
        //==================================================

        // アイテムフォームを作成するメソッド
        function createItemForm(loopMax, loopStartIndex) {
            for (let i = loopStartIndex, html; i < loopMax; i++) {
                html = itemContainer.innerHTML;
                itemContainer.innerHTML = html +
                    '<div class="row py-3 mb-2 item_ele">' +
                    '<div class="col-md-4">' +
                    '<label for="state" class="form-label">[' + (i + 1) + ']アイテムの種類</label>' +
                    '<select class="form-select" id="state" name="item_id' + (i + 1) + '" required>' +
                    @if(!empty($items))
                        @foreach($items as $item)
                        '<option value="{{$item['id']}}">{{$item['name']}}</option>' +
                    @endforeach
                        @endif
                        '</select>' +
                    '</div>' +
                    '<div class="col-md-2">' +
                    '<label for="number" class="form-label">[' + (i + 1) + ']アイテムの個数</label>' +
                    '<input type="number" id="number" name="item_cnt' + (i + 1) + '" class="form-control" min="1" value="1" required>' +
                    '</div>'
                '</div><br>';
            }
        }

        const itemContainer = document.getElementById('item_container');    // アイテムフォームの格納先
        const inputNumber = document.getElementById('type_cnt');              // アイテムの種類数

        // 値の変更時にイベントを呼ぶ
        inputNumber.addEventListener('change', (e) => {

            if (isNaN(inputNumber.value)) return;   // stringからintに変換できない場合

            // stringからintに変換する
            let value = Number(inputNumber.value);
            // 現在存在するアイテムのフォームを取得する
            const children = document.querySelectorAll(".item_ele");

            let loopMax = 0;            // アイテムフォームを作成するときにループする条件
            let loopStartIndex = 0;         // indexの初めの値

            // 子要素が存在する場合
            if (children.length > 0) {
                // 現在のアイテム要素の数より入力した値が小さい場合
                if (children.length > value) {
                    for (let i = children.length; i > value; i--) {
                        // 不必要な子要素を削除する
                        children[i - 1].remove();
                    }
                }
                // 現在のアイテム要素の数より入力した値が大きい場合
                else if (children.length < value) {
                    loopMax = value;
                    loopStartIndex = children.length;
                }
                // 現在のアイテム要素の数と入力した値が同じ場合
                else {
                    return;
                }
            } else {
                loopMax = value;
                loopStartIndex = 0;
            }

            // アイテムフォームを作成する
            createItemForm(loopMax, loopStartIndex);

            //----------------------------------------------
            // アイテムの種類数を入力時にcollapseを表示・非表示にする
            //----------------------------------------------
            const collapse = document.getElementById('collapseItems')

            collapse.classList.remove('show');
            if (value > 0) {
                collapse.classList.add('show');
            }
        });

    </script>
@endsection
