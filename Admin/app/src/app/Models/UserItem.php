<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class UserItem extends Model
{
    // $guardedには更新しないカラムを指定する
    protected $guarded = [
        'id',
    ];
}
