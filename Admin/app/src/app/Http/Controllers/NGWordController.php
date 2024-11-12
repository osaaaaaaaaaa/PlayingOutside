<?php

namespace App\Http\Controllers;

use App\Models\NGWord;
use Illuminate\Http\Request;

class NGWordController extends Controller
{
    public function index(Request $request)
    {
        // アカウントテーブルから全てのレコードを取得する
        $ngwords = NGWord::paginate(20);
        return view('ngwords/index', ['ngwords' => $ngwords]);
    }
}
