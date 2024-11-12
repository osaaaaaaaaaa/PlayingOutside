<?php

namespace App\Http\Controllers;

use App\Models\Item;
use Illuminate\Http\Request;

class ItemController
{
    // アイテム一覧表示
    public function index(Request $request)
    {
        // アカウントテーブルから全てのレコードを取得する
        $items = Item::paginate(20);
        return view('items/index', ['items' => $items]);
    }
}
