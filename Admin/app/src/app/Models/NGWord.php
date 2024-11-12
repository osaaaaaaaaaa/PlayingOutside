<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class NGWord extends Model
{
    protected $table = 'ng_words';

    // $guardedには更新しないカラムを指定する
    protected $guarded = [
        'id',
    ];
}
