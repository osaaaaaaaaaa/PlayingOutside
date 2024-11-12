<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Factories\HasFactory;
use Illuminate\Database\Eloquent\Model;

class Account extends Model
{
    // $guardedには更新しないカラムを指定する
    protected $guarded = [
        'id',
    ];
}
