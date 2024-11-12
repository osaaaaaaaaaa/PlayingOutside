<?php

namespace App\Http\Controllers;

use App\Models\Constant;
use Illuminate\Http\Request;

class ConstantController extends Controller
{
    public function index(Request $request)
    {
        // アカウントテーブルから全てのレコードを取得する
        $constants = Constant::paginate(20);
        for ($i = 0; $i < count($constants); $i++) {
            switch ($constants[$i]['type']) {
                case 1:
                    $constants[$i]['type'] = 'ステージ最大数';
                    break;
                case 2:
                    $constants[$i]['type'] = 'フォロー最大人数';
                    break;
            }
        }

        return view('constants/index', ['constants' => $constants]);
    }
}
