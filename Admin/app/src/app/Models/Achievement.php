<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class Achievement extends Model
{
    // $guardedには更新しないカラムを指定する
    protected $guarded = [
        'id',
    ];

    // アイテムのリレーション
    public function items()
    {
        // 第二引数：子モデルのカラム,第三引数：親モデルのカラム
        return $this->hasMany(Item::class, 'id', 'item_id');
    }
}
